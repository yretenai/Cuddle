using System;
using Cuddle.Core.VFS;
using DragonLib;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core.Structs.FileSystem;

// umodel reference: https://github.com/gildor2/UEViewer/blob/c444911a6ad65bff5266f273dd5bdf7dd6fb506e/Unreal/FileSystem/UnArchivePak.h#L69
public sealed record FPakEntry : IVFSEntry {
    public FPakEntry() => Hash = new Lazy<byte[]>(new byte[0x14]);

    public FPakEntry(FArchiveReader archive, FPakFile owner, bool isCompressed) {
        Owner = owner;

        if (isCompressed) {
            // var fields = BitPacked.Unpack<FPakEntryFlags>(archive.Read<uint>());
            var fields = new FPakEntryEncodedFlags(archive.Read<uint>());

            if (fields.CompressionBlockSize == 0x3F) {
                CompressionBlockSize = archive.Read<uint>();
            } else {
                CompressionBlockSize = (uint) fields.CompressionBlockSize << 11;
            }

            CompressionMethod = fields.CompressionMethod;

            Pos = fields.OffsetSizeIs32BitSafe ? archive.Read<uint>() : archive.Read<long>();
            UncompressedSize = fields.UncompressedSizeIs32BitSafe ? archive.Read<uint>() : archive.Read<long>();
            Size = CompressionMethod > 0 ? fields.SizeIs32BitSafe ? archive.Read<uint>() : archive.Read<long>() : UncompressedSize;
            IsEncrypted = fields.Encrypted;

            Hash = new Lazy<byte[]>(() => {
                using var hash = Owner.ReadBytes(Pos, 0x30, IsEncrypted)[^0x14..];
                return hash.Span.ToArray();
            });

            // we read the entire data block from after the lead-in struct.
            Pos += 53; // no version checks because this can only exist after UE 4.23

            // skip compression information
            if (CompressionMethod > 0) {
                Pos += 4 + fields.CompressionBlockCount * 0x10;
            }

            CompressionBlocks = new FPakCompressedBlock[fields.CompressionBlockCount];
            if (fields.CompressionBlockCount > 0) {
                if (UncompressedSize < CompressionBlockSize || fields.CompressionBlockSize < 0x3F && UncompressedSize < 0x10000) {
                    CompressionBlockSize = (uint) UncompressedSize;
                }

                if (fields.CompressionBlockCount == 1) {
                    CompressionBlocks[0] = new FPakCompressedBlock { CompressedStart = 0, CompressedEnd = Size };
                } else {
                    var current = 0L;
                    for (var index = 0; index < fields.CompressionBlockCount; ++index) {
                        var size = archive.Read<uint>();
                        CompressionBlocks[index] = new FPakCompressedBlock { CompressedStart = current, CompressedEnd = current + size };
                        if (IsEncrypted) {
                            size = size.Align(16);
                        }

                        current += size;
                    }
                }
            }
        } else {
            var start = archive.Position;

            Pos = archive.Read<long>();
            Size = archive.Read<long>();
            UncompressedSize = archive.Read<long>();
            CompressionMethod = archive.Game.GetEngineVersion() is EGame.UE4_22 ? archive.Read<byte>() : archive.Read<int>();
            Timestamp = owner.Version < EPakVersion.NoTimestamps ? archive.Read<long>() : 0;
            Hash = new Lazy<byte[]>(archive.ReadArray<byte>(0x14).ToArray());

            if (owner.Version >= EPakVersion.CompressionEncryption) {
                if (CompressionMethod > 0) {
                    CompressionBlocks = archive.ReadArray<FPakCompressedBlock>().ToArray();
                }

                var flags = archive.Read<FPakEntryFlags>();
                IsEncrypted = flags.HasFlag(FPakEntryFlags.Encrypted);
                IsDeleted = flags.HasFlag(FPakEntryFlags.Deleted);
                CompressionBlockSize = archive.Read<uint>();
            }

            var delta = archive.Position - start;
            Pos += delta;

            // we read the entire data block from after the lead-in struct.
            if (owner.Version >= EPakVersion.RelativeChunkOffsets) {
                // substract lead-in offset
                for (var index = 0; index < CompressionBlocks.Length; ++index) {
                    CompressionBlocks[index].CompressedStart -= delta;
                    CompressionBlocks[index].CompressedEnd -= delta;
                }
            } else {
                // subtract global offset
                for (var index = 0; index < CompressionBlocks.Length; ++index) {
                    CompressionBlocks[index].CompressedStart -= Pos;
                    CompressionBlocks[index].CompressedEnd -= Pos;
                }
            }
        }
    }

    public long Pos { get; }
    public long UncompressedSize { get; }
    public int CompressionMethod { get; }
    public long Timestamp { get; }
    public Lazy<byte[]> Hash { get; }
    public FPakCompressedBlock[] CompressionBlocks { get; } = Array.Empty<FPakCompressedBlock>();
    public bool IsEncrypted { get; }
    public bool IsDeleted { get; }
    public uint CompressionBlockSize { get; }

    public IVFSFile Owner { get; } = null!;
    public long Size { get; }
    public string MountedPath { get; internal set; } = "";
    public string ObjectPath { get; internal set; } = "";
    public ulong MountedHash { get; internal set; }
    public IPoliteDisposable? Data { get; set; }
    public bool Disposed { get; private set; }

    public MemoryOwner<byte> ReadFile() => Owner.ReadFile(this);

    public void Dispose() {
        if (Data is IDisposable disposable) {
            disposable.Dispose();
        }

        Data = null;

        if (Disposed) {
            return;
        }

        GC.SuppressFinalize(this);
        Disposed = true;
    }

    public void Reset() {
        if (Data is IDisposable disposable) {
            disposable.Dispose();
        }

        Data = null;

        if (Disposed) {
            GC.ReRegisterForFinalize(this);
            Disposed = false;
        }
    }

    ~FPakEntry() {
        Dispose();
    }

    public override string ToString() => MountedPath;

    // ref: https://github.com/gildor2/UEViewer/blob/eaba2837228f9fe39134616d7bff734acd314ffb/Unreal/FileSystem/FileSystemUtils.cpp#L20
    public static string CreateObjectPath(string mountedPath) {
        // Engine/Content/Stuff -> /Engine/Stuff -- normalize Engine
        if (mountedPath.StartsWith("Engine/Content")) {
            return "/Engine" + mountedPath[14..];
        }

        // Engine/Plugins/Stuff -> /Plugins/Stuff -- normalize Plugins
        if (mountedPath.StartsWith("Engine/Plugins")) {
            mountedPath = mountedPath[6..];
            var contentIndex = mountedPath.IndexOf("/Content", StringComparison.Ordinal);
            var contentLength = 8;
            if (contentIndex == -1) {
                contentIndex = mountedPath.IndexOf("/Config", StringComparison.Ordinal);
                contentLength = 0;
            }

            if (contentIndex == -1) {
                if (mountedPath.EndsWith(".uplugin")) {
                    var uplugin = mountedPath[(mountedPath.LastIndexOf('/') + 1)..];
                    return $"/Plugins/{uplugin[..^8]}/{uplugin}";
                }

                return mountedPath;
            }

            var prefix = mountedPath[..contentIndex];
            var name = prefix[(prefix.LastIndexOf('/') + 1)..];
            return $"/Plugins/{name}{mountedPath[(contentIndex + contentLength)..]}";
        }

        // Engine/Stuff -> /Engine/Stuff -- normalize Engine
        if (mountedPath.StartsWith("Engine/")) {
            return "/" + mountedPath;
        }

        // Project/Content/Stuff/ -> Game/Content/Stuff -- strip project nname
        var index = mountedPath.IndexOf('/');
        if (index == -1) {
            return mountedPath;
        }

        var objectPath = mountedPath;
        if (mountedPath[..index] != "Game") {
            objectPath = mountedPath[(index + 1)..];
        }

        // Content/Stuff/ -> Stuff -- strip Content directory
        if (objectPath.StartsWith("Content/")) {
            objectPath = objectPath[8..];
        }

        // Stuff.uasset -> Stuff -- strip package extensions
        var extPos = objectPath.LastIndexOf('.');
        if (extPos > -1) {
            var ext = objectPath[extPos..];
            if (ext is ".uasset" or ".umap") {
                objectPath = objectPath[..^ext.Length];
            }
        }

        // Stuff -> /Game/Stuff -- rebuild path
        return "/Game/" + objectPath;
    }
}
