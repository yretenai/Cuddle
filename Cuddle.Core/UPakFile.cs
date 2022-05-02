using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using Cuddle.Core.Enums;
using Cuddle.Core.Structs.FileSystem;
using DragonLib;
using K4os.Compression.LZ4;
using Microsoft.Toolkit.HighPerformance;
using Serilog;
using ZstdNet;

namespace Cuddle.Core;

public sealed class UPakFile : IDisposable {
    public UPakFile(Stream stream, EGame game, string name, AESKeyStore? keyStore = null, HashPathStore? hashStore = null) {
        Name = name;

        var offset = 0x3D;

        if (game >= EGame.UE4_22) {
            offset += 0x80;
        }

        if (game >= EGame.UE4_23) {
            offset += 0x20;
        }

        if (game.GetEngineVersion() is EGame.UE4_25) {
            // I tried to track what caused this in UE code, but i failed.
            // for now, trust UEViewer.
            offset += 1; // ????
        }

        var buffer = new byte[offset].AsMemory();
        stream.Seek(-offset, SeekOrigin.End);
        if (stream.Read(buffer.Span) != offset) {
            Log.Error("Failed to read PakFile header for {PakName}! Stream is too small", Name);
            return;
        }

        var header = new FArchive(game, buffer);

        EncryptionGuid = header.Read<Guid>();
        IsIndexEncrypted = header.Read<byte>() != 0;
        Tag = header.Read<uint>();
        if (Tag != 0x5A6F12E1) {
            Log.Error("Failed to read PakFile header for {PakName}! Magic is invalid, expected 5A6F12E1 but got {Tag:X}", Name, Tag);
            return;
        }

        Stream = stream;

        Version = header.Read<EPakVersion>();

        if (Version < EPakVersion.IndexEncryption) {
            IsIndexEncrypted = false;
        }

        if (Version < EPakVersion.EncryptionKeyGuid) {
            EncryptionGuid = Guid.Empty;
        }

        SubVersion = header.Read<ushort>();
        var indexOffset = header.Read<long>();
        var indexSize = header.Read<long>();
        IndexHash = header.ReadArray<byte>(0x14).ToArray();

        if (Version is EPakVersion.FrozenIndex) {
            IndexIsFrozen = header.Read<byte>() != 0;
        }

        if (Version < EPakVersion.FNameBasedCompressionMethod) {
            CompressionMethods = new List<string> {
                "None", // COMPRESS_None
                "Zlib", // COMPRESS_ZLIB
                "Gzip", // COMPRESS_GZIP
                "Custom", // This was never defined, but some games defined it as Oodle.
                "Custom", // COMPRESS_Custom, but it's probably Oodle (that's what the UE source assumes.) -- Validate headers, if nothing works it's probably LZ4
            };
        } else {
            CompressionMethods = new List<string> {
                "None",
            };

            var count = 4;
            if (game >= EGame.UE4_23) {
                count += 1;
            }

            for (var index = 0; index < count; ++index) {
                CompressionMethods.Add(header.ReadArray<byte>(0x20).Span.ReadString() ?? "None");
            }
        }

        if (IsIndexEncrypted) {
            var testBlock = new byte[16].AsMemory();
            Stream.Position = indexOffset;
            if (Stream.Read(testBlock.Span) != 16) {
                Log.Error("Failed reading encryption test block for PAK {PakName}", Name);
                // ????
                return;
            }

            if (keyStore == null || !FindEncryptionKey(keyStore, testBlock)) {
                Log.Error("Can't find encryption key that suits Encryption Key GUID {KeyGuid} for PAK {PakName}", EncryptionGuid, Name);
                return;
            }
        }

        Index = new FPakIndex(new FArchive(game, ReadBytes(indexOffset, indexSize, IsIndexEncrypted)), this, hashStore);
    }

    public List<string> CompressionMethods { get; } = null!;

    public string Name { get; }
    private Stream? Stream { get; }
    public Guid EncryptionGuid { get; }
    private byte[]? EncryptionKey { get; set; }
    public bool IsIndexEncrypted { get; }
    public uint Tag { get; }
    public EPakVersion Version { get; }
    public ushort SubVersion { get; }
    public byte[]? IndexHash { get; }
    public bool IndexIsFrozen { get; }
    public FPakIndex Index { get; } = null!;

    public void Dispose() {
        Stream?.Dispose();
    }

    public Memory<byte> ReadFile(string path) {
        var index = Index.Files.FirstOrDefault(x => x.MountedPath == path);
        if (index == null) {
            return Memory<byte>.Empty;
        }

        return ReadFile(index);
    }

    public Memory<byte> ReadFile(FPakEntry index) {
        var dataBuffer = ReadBytes(index.Pos, index.Size, index.IsEncrypted);

        if (index.CompressionMethod == 0) {
            return dataBuffer;
        }
        
        var outputBuffer = new byte[index.UncompressedSize].AsMemory();

        var lastBlockIndex = index.CompressionBlocks.Length - 1;
        var outputOffset = 0L;
        for (var i = 0; i < index.CompressionBlocks.Length; i++) {
            var block = index.CompressionBlocks[i];
            var size = i == lastBlockIndex ? index.UncompressedSize - outputOffset : index.CompressionBlockSize;
            var blockChunk = dataBuffer[(int) block.CompressedStart..(int)block.CompressedEnd];

            var compressionType = CompressionMethods[index.CompressionMethod].ToLower();

            if (compressionType == "magic") {
                if ((BinaryPrimitives.ReadUInt16LittleEndian(blockChunk.Span) & 0xFFFFFF) == 0xb52ffd) {
                    compressionType = "zstd";
                } else if (blockChunk.Span[0] == 0b1111000) {
                    compressionType = "zlib";
                } else if (BinaryPrimitives.ReadUInt16LittleEndian(blockChunk.Span) == 0x8b1f) {
                    compressionType = "gzip";
                } else if ((blockChunk.Span[0] & 0x7F) == 0b1100 && (blockChunk.Span[1] & 0x7F) < 15) {
                    // Oodle compression magic:
                    // 7654 3210 | 7654 3210
                    // ABBB CCCC | DEEE EEEE
                    
                    // A = restart decoder after frame
                    // B = reserved
                    // C = magic bits
                    // D = use checksums
                    // E = encoder 0~14 { LZH, LZHLW, LZNIB, None, LZB16, LZBLW, LZA, LZNA, Kraken, Mermaid, BitKnit, Selkie, Hydra, Leviathan, Akkorokamui } as of oo2core_9
                    compressionType = "oodle";
                } else {
                    compressionType = "lz4";
                }
            }

            var blockData = Memory<byte>.Empty;
            switch (compressionType) {
                case "zlib": {
                    using var dataStream = blockChunk.AsStream();
                    dataStream.Position = 2;
                    using var zlib = new DeflateStream(dataStream, CompressionMode.Decompress);
                    var offset = 0;
                    blockData = new byte[size].AsMemory();
                    while (blockData.Length - offset > 0) {
                        var amount = zlib.Read(blockData[offset..].Span);
                        if (amount == 0) {
                            break;
                        }

                        offset += amount;
                    }
                    break;
                }
                case "zstd": {
                    using var zstd = new Decompressor();
                    blockData = zstd.Unwrap(blockChunk.Span, (int) size);
                    break;
                }
                case "gzip": {
                    using var dataStream = blockChunk.AsStream();
                    dataStream.Position = 2;
                    using var zlib = new GZipStream(dataStream, CompressionMode.Decompress);
                    var offset = 0;
                    blockData = new byte[size].AsMemory();
                    while (blockData.Length - offset > 0) {
                        var amount = zlib.Read(blockData[offset..].Span);
                        if (amount == 0) {
                            break;
                        }

                        offset += amount;
                    }
                    break;
                }
                case "oodle": {
                    if (!Oodle.IsReady) {
                        Log.Error("Unable to decompress file {Path} because it uses Oodle compression and the Oodle dll has not been loaded!", index.Path);
                    }
                    blockData = Oodle.Decompress(blockChunk, (int) size);
                    break;
                }
                case "lz4": {
                    blockData = new byte[size].AsMemory();
                    LZ4Codec.Decode(blockChunk.Span, blockData.Span);
                    break;
                }
            }

            blockData.CopyTo(outputBuffer[(int)outputOffset..]);
            outputOffset += blockData.Length;
        }

        return outputBuffer;
    }

    internal Memory<byte> ReadBytes(long offset, long count, bool isEncrypted) {
        if (Stream is not { CanRead: true }) {
            return Memory<byte>.Empty;
        }

        var data = new byte[count < 16 ? 16 : count].AsMemory();
        Stream.Position = offset;
        var readOffset = 0;
        while (count - readOffset > 0) {
            var amount = Stream.Read(data.Span[readOffset..]);
            if (amount == 0) {
                break; // can't read anymore.
            }

            readOffset += amount;
        }

        var decrypted = Decrypt(data, isEncrypted);
        if (count < 16) { // aes needs 16 bytes.
            return decrypted[..(int) count];
        }

        return decrypted;
    }

    private bool FindEncryptionKey(AESKeyStore aesKey, Memory<byte> test) {
        if (aesKey.Keys.TryGetValue(EncryptionGuid, out var key)) {
            EncryptionKey = key;
            return true;
        }

        foreach (var unknownKey in aesKey.NullKeys) {
            EncryptionKey = unknownKey;
            var data = Decrypt(test, true);
            if (Math.Abs(BinaryPrimitives.ReadInt32LittleEndian(data.Span)) < 255) {
                aesKey.Keys[EncryptionGuid] = EncryptionKey;
                return true;
            }
        }

        EncryptionKey = null;
        return false;
    }

    private Memory<byte> Decrypt(Memory<byte> data, bool isEncrypted) {
        if (!isEncrypted || EncryptionKey == null) {
            return data;
        }

        using var cipher = Aes.Create();
        cipher.Mode = CipherMode.ECB;
        cipher.Padding = PaddingMode.None;
        cipher.BlockSize = 128;
        cipher.Key = EncryptionKey;
        cipher.IV = new byte[16];
        using var decrypt = cipher.CreateDecryptor();
        return decrypt.TransformFinalBlock(data.ToArray(), 0, data.Length);
    }
}
