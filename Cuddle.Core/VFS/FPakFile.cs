using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.FileSystem;
using Cuddle.Security;
using DragonLib;
using IronCompress;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Serilog;

namespace Cuddle.Core.VFS;

public sealed class FPakFile : IVFSFile {
    public FPakFile(string fullPath, EGame game, string name, AESKeyStore? keyStore, HashPathStore? hashStore, VFSManager manager) {
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
        stream.ReadExactly(buffer.Span);

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

        if (IsIndexEncrypted || EncryptionGuid != Guid.Empty && EncryptionKey != null) {
            var testBlock = new byte[16].AsSpan();
            stream.Position = indexOffset;
            stream.ReadExactly(testBlock);

            if (keyStore == null || !keyStore.FindEncryptionKey(EncryptionGuid, testBlock, out var enc)) {
                Log.Error("Can't find encryption key that suits Encryption Key GUID {KeyGuid} for PAK {PakName}", EncryptionGuid, Name);
                return;
            }

            EncryptionKey = enc;
        }

        using var indexReader = new FArchiveReader(game, ReadBytes(indexOffset, indexSize, IsIndexEncrypted));
        Index = new FPakIndex(indexReader, this, hashStore);

        if (IsIndexEncrypted || EncryptionGuid != Guid.Empty) {
            if (EncryptionGuid == Guid.Empty) {
                Log.Information("Mounted VFS Pak {Name} on \"{MountPoint}\" ({Count} files, encryption key is {Present})", Name, Index.MountPoint, Index.Files.Count, EncryptionKey == null ? "not present" : "present");
            } else {
                Log.Information("Mounted VFS Pak {Name} on \"{MountPoint}\" ({Count} files, encryption key {EncryptionGuid:n} is {Present})", Name, Index.MountPoint, Index.Files.Count, EncryptionGuid, EncryptionKey == null ? "not present" : "present");
            }
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
    public IEnumerable<IVFSEntry> Entries => Index?.Files.Where(x => !x.IsDeleted) ?? new List<FPakEntry>();
    private static Iron Iron { get; set; } = new();

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
        if (vfsIndex is not FPakEntry index) {
            throw new InvalidOperationException();
        }

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
            var copy = true;

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
                    using var zlib = new ZLibStream(dataStream, CompressionMode.Decompress);
                    zlib.ReadExactly(blockData.Span);
                    break;
                }
                case "gzip": {
                    using var dataPin = blockChunk.Pin();
                    using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, blockChunk.Length);
                    dataStream.Position = 2;
                    using var zlib = new GZipStream(dataStream, CompressionMode.Decompress);
                    zlib.ReadExactly(blockData.Span);

                    break;
                }
                case "oodle": {
                    if (Manager.Oodle == null) {
                        Log.Error("Unable to decompress file {Path} because it uses Oodle compression and the Oodle library has not been loaded!", index.MountedPath);
                        throw new InvalidOperationException();
                    }

                    if (Manager.Oodle.Decompress(blockChunk, blockData) == -1) {
                        throw new InvalidOperationException();
                    }

                    break;
                }
                case "zstd":
                case "lz4": {
                    copy = false;
                    var codec = compressionType == "zstd" ? Codec.Zstd : Codec.LZ4;
                    using var data = Iron.Decompress(codec, blockChunk.Span, (int) size);
                    data.AsSpan().CopyTo(outputBuffer.Span[(int) outputOffset..]);
                    break;
                }
            }

            if (copy) {
                blockData.CopyTo(outputBuffer.Memory[(int) outputOffset..]);
            }

            outputOffset += size;
        }

        dataBuffer.Dispose();
        return outputBuffer;
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

    public MemoryOwner<byte> ReadBytes(long offset, long count, bool isEncrypted) {
        using var stream = new FileStream(FullPath, FileMode.Open, FileAccess.ReadWrite);

        // aes needs 16 byte aligned data.
        var data = MemoryOwner<byte>.Allocate((int) count.Align(16));
        stream.Position = offset;
        stream.ReadExactly(data.Span);
        return Decrypt(data, isEncrypted)[..(int) count];
    }

    ~FPakFile() {
        Dispose();
    }

    private MemoryOwner<byte> Decrypt(MemoryOwner<byte> data, bool isEncrypted) {
        if (!isEncrypted || EncryptionKey == null) {
            return data;
        }

        var decryptedOwner = MemoryOwner<byte>.Allocate(data.Length);
        var size = AESKeyStore.Decrypt(EncryptionKey, data.Span, decryptedOwner.Span);
        data.Dispose();
        return decryptedOwner[..size];
    }
}
