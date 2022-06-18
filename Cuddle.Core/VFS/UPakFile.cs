using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using Cuddle.Core.Assets;
using Cuddle.Core.Enums;
using Cuddle.Core.Structs.FileSystem;
using DragonLib;
using K4os.Compression.LZ4;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Serilog;
using ZstdNet;

namespace Cuddle.Core.VFS;

public sealed class UPakFile : IVFSFile {
    public UPakFile(string fullPath, EGame game, string name, AESKeyStore? keyStore, HashPathStore? hashStore, VFSManager manager) {
        Name = name;
        Game = game;
        Manager = manager;

        using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

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

        using var buffer = MemoryOwner<byte>.Allocate(offset);
        stream.Seek(-offset, SeekOrigin.End);
        if (stream.Read(buffer.Span) != offset) {
            Log.Error("Failed to read PakFile header for {PakName}! Stream is too small", Name);
            return;
        }

        using var header = new FArchiveReader(game, buffer);

        EncryptionGuid = header.Read<Guid>();
        IsIndexEncrypted = header.Read<byte>() != 0;
        Tag = header.Read<uint>();
        if (Tag != 0x5A6F12E1) {
            Log.Error("Failed to read PakFile header for {PakName}! Magic is invalid, expected 5A6F12E1 but got {Tag:X}", Name, Tag);
            return;
        }

        FullPath = fullPath;

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
                CompressionMethods.Add(header.ReadArray<byte>(0x20).ReadString() ?? "None");
            }
        }

        if (IsIndexEncrypted || EncryptionGuid != Guid.Empty) {
            using var testBlock = MemoryOwner<byte>.Allocate(16);
            stream.Position = indexOffset;
            if (stream.Read(testBlock.Span) != 16) {
                Log.Error("Failed reading encryption test block for PAK {PakName}", Name);
                // ????
                return;
            }

            if (keyStore == null || !FindEncryptionKey(keyStore, testBlock)) {
                Log.Error("Can't find encryption key that suits Encryption Key GUID {KeyGuid} for PAK {PakName}", EncryptionGuid, Name);
                return;
            }
        }

        using var indexReader = new FArchiveReader(game, ReadBytes(indexOffset, indexSize, IsIndexEncrypted));
        Index = new FPakIndex(indexReader, this, hashStore);

        if (IsIndexEncrypted || EncryptionGuid != Guid.Empty) {
            Log.Information("Mounted VFS Pak {Name} on \"{MountPoint}\" ({Count} files, key {EncryptionGuid:n} which is {Present})", Name, Index?.MountPoint ?? "None", Index?.Files?.Count ?? -1, EncryptionGuid, EncryptionKey == null ? "not present" : "present");
        } else {
            Log.Information("Mounted VFS Pak {Name} on \"{MountPoint}\" ({Count} files)", Name, Index.MountPoint, Index.Files.Count);
        }
    }

    public List<string> CompressionMethods { get; } = null!;
    private string FullPath { get; } = null!;
    public bool IsIndexEncrypted { get; }
    public uint Tag { get; }
    public EPakVersion Version { get; }
    public ushort SubVersion { get; }
    public byte[]? IndexHash { get; }
    public bool IndexIsFrozen { get; }
    public FPakIndex? Index { get; }
    public VFSManager Manager { get; }

    public string Name { get; }
    public EGame Game { get; }
    public Guid EncryptionGuid { get; }
    public byte[]? EncryptionKey { get; set; }
    public bool HasHashes => Version >= EPakVersion.PathHashIndex;
    public bool HasPaths => true;
    public IEnumerable<IVFSEntry> Entries => Index?.Files ?? new List<FPakEntry>();

    public MemoryOwner<byte> ReadFile(string path) {
        var index = Index?.Files.FirstOrDefault(x => x.MountedPath == path || x.ObjectPath.EndsWith(path, StringComparison.Ordinal));
        return index == null ? MemoryOwner<byte>.Empty : ReadFile(index);
    }

    public MemoryOwner<byte> ReadFile(ulong hash) {
        if (!HasHashes) {
            return MemoryOwner<byte>.Empty;
        }

        var index = Index?.Files.FirstOrDefault(x => x.MountedHash == hash);
        return index == null ? MemoryOwner<byte>.Empty : ReadFile(index);
    }

    public unsafe MemoryOwner<byte> ReadFile(IVFSEntry vfsIndex) {
        var index = (FPakEntry) vfsIndex;
        var dataBuffer = ReadBytes(index.Pos, index.Size, index.IsEncrypted);

        if (index.CompressionMethod == 0) {
            return dataBuffer;
        }

        var outputBuffer = MemoryOwner<byte>.Allocate((int) index.UncompressedSize);

        var lastBlockIndex = index.CompressionBlocks.Length - 1;
        var outputOffset = 0L;
        using var blockDataRoot = MemoryOwner<byte>.Allocate((int) index.CompressionBlockSize);
        for (var i = 0; i < index.CompressionBlocks.Length; i++) {
            var block = index.CompressionBlocks[i];
            var size = i == lastBlockIndex ? index.UncompressedSize - outputOffset : index.CompressionBlockSize;
            if (size > blockDataRoot.Length) {
                throw new InvalidOperationException();
            }

            var blockData = blockDataRoot.Memory[..(int) size];
            var blockChunk = dataBuffer.Memory[(int) block.CompressedStart..(int) block.CompressedEnd];

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

            switch (compressionType) {
                case "zlib": {
                    using var dataPin = blockChunk.Pin();
                    using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, blockChunk.Length);
                    dataStream.Position = 2;
                    using var zlib = new DeflateStream(dataStream, CompressionMode.Decompress);
                    var offset = 0;
                    while (size - offset > 0) {
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
                    if (zstd.Unwrap(blockChunk.Span, blockData.Span) < 0) {
                        throw new InvalidOperationException();
                    }

                    break;
                }
                case "gzip": {
                    using var dataPin = blockChunk.Pin();
                    using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, blockChunk.Length);
                    dataStream.Position = 2;
                    using var zlib = new GZipStream(dataStream, CompressionMode.Decompress);
                    var offset = 0;
                    while (size - offset > 0) {
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

                    if (Oodle.Decompress(blockChunk, blockData) == -1) {
                        throw new InvalidOperationException();
                    }

                    break;
                }
                case "lz4": {
                    if (LZ4Codec.Decode(blockChunk.Span, blockData.Span) == -1) {
                        throw new InvalidOperationException();
                    }

                    break;
                }
            }

            blockData.CopyTo(outputBuffer.Memory[(int) outputOffset..]);
            outputOffset += size;
        }

        dataBuffer.Dispose();
        return outputBuffer;
    }

    public UAssetFile? ReadAsset(string path) {
        if (Disposed) {
            return null;
        }

        var index = Index?.Files.FirstOrDefault(x => x.MountedPath.Equals(path, StringComparison.Ordinal) || x.ObjectPath.EndsWith(path, StringComparison.Ordinal));
        return index == null ? null : ReadAsset(index);
    }

    public UAssetFile? ReadAsset(ulong hash) {
        if (Disposed || !HasHashes) {
            return null;
        }

        var index = Index?.Files.FirstOrDefault(x => x.MountedHash == hash);
        return index == null ? null : ReadAsset(index);
    }

    public UAssetFile? ReadAsset(IVFSEntry entry) {
        if (Disposed) {
            return null;
        }

        if (entry.Disposed) {
            entry.Reset();
        }

        if (entry.Data is null) {
            var data = ReadFile(entry);
            if (data.Length == 0) {
                return null;
            }

            var uexp = ReadFile(Path.ChangeExtension(entry.MountedPath, ".uexp"));
            entry.Data = new UAssetFile(data, uexp, Path.GetFileNameWithoutExtension(entry.MountedPath), Game, this, Manager);
        }

        return (UAssetFile) entry.Data;
    }

    public UObject? ReadAssetExport(string path, int export) {
        if (Disposed) {
            return null;
        }

        var index = Index?.Files.FirstOrDefault(x => x.MountedPath.Equals(path, StringComparison.Ordinal) || x.ObjectPath.EndsWith(path, StringComparison.Ordinal));
        return index == null ? null : ReadAssetExport(index, export);
    }

    public UObject? ReadAssetExport(ulong hash, int export) {
        if (Disposed || !HasHashes) {
            return null;
        }

        var index = Index?.Files.FirstOrDefault(x => x.MountedHash == hash);
        return index == null ? null : ReadAssetExport(index, export);
    }

    public UObject? ReadAssetExport(IVFSEntry entry, int export) => Disposed ? null : ReadAsset(entry)?.GetExport(export);

    public UObject?[] ReadAssetExports(string path) {
        if (Disposed) {
            return Array.Empty<UObject>();
        }

        var index = Index?.Files.FirstOrDefault(x => x.MountedPath.Equals(path, StringComparison.Ordinal) || x.ObjectPath.EndsWith(path, StringComparison.Ordinal));
        return index == null ? Array.Empty<UObject>() : ReadAssetExports(index);
    }

    public UObject?[] ReadAssetExports(ulong hash) {
        if (Disposed || !HasHashes) {
            return Array.Empty<UObject>();
        }

        var index = Index?.Files.FirstOrDefault(x => x.MountedHash == hash);
        return index == null ? Array.Empty<UObject>() : ReadAssetExports(index);
    }

    public UObject?[] ReadAssetExports(IVFSEntry entry) => (Disposed ? null : ReadAsset(entry)?.GetExports()) ?? Array.Empty<UObject?>();

    public bool FindEncryptionKey(AESKeyStore aesKey, MemoryOwner<byte> test) {
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

    public void ClearCaches() {
        foreach (var entry in Entries) {
            if (entry.Data is IDisposable disposable) {
                disposable.Dispose();
            }

            entry.Data = null;
        }
    }

    public bool Disposed { get; private set; }

    public void Dispose() {
        foreach (var entry in Entries) {
            entry.Dispose();
        }

        if (Disposed) {
            return;
        }

        GC.SuppressFinalize(this);
        Disposed = true;
    }

    ~UPakFile() {
        Dispose();
    }

    internal MemoryOwner<byte> ReadBytes(long offset, long count, bool isEncrypted) {
        using var stream = new FileStream(FullPath, FileMode.Open, FileAccess.ReadWrite);

        // aes needs 16 byte aligned data.
        var data = MemoryOwner<byte>.Allocate((int) count.Align(16));
        stream.Position = offset;
        var readOffset = 0;
        while (count - readOffset > 0) {
            var amount = stream.Read(data.Span[readOffset..]);
            if (amount == 0) {
                break; // can't read anymore.
            }

            readOffset += amount;
        }

        return Decrypt(data, isEncrypted)[..(int) count];
    }

    private MemoryOwner<byte> Decrypt(MemoryOwner<byte> data, bool isEncrypted) {
        if (!isEncrypted || EncryptionKey == null) {
            return data;
        }

        using var cipher = Aes.Create();
        cipher.Mode = CipherMode.ECB;
        cipher.Padding = PaddingMode.None;
        cipher.BlockSize = 128;
        cipher.Key = EncryptionKey;
        cipher.IV = new byte[16];
        var decryptedOwner = MemoryOwner<byte>.Allocate(data.Length);
        var size = cipher.DecryptEcb(data.Span, decryptedOwner.Span, cipher.Padding);
        data.Dispose();
        return decryptedOwner[..size];
    }
}
