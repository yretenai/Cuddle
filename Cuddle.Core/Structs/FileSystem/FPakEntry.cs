using System;
using Cuddle.Core.Assets;
using Cuddle.Core.VFS;
using DragonLib;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core.Structs.FileSystem;

// umodel reference: https://github.com/gildor2/UEViewer/blob/c444911a6ad65bff5266f273dd5bdf7dd6fb506e/Unreal/FileSystem/UnArchivePak.h#L69
public sealed record FPakEntry : IVFSEntry {
    public FPakEntry() => Hash = new byte[0x14];

    public FPakEntry(FArchiveReader archive, UPakFile owner, bool isCompressed) {
        Owner = owner;

        if (isCompressed) {
            // var fields = BitPacked.Unpack<FPakEntryFlags>(archive.Read<uint>());
            var fields = new FPakEntryFlags(archive.Read<uint>());

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

            // read hash from lead-in struct
            using var hash = owner.ReadBytes(Pos, 0x30, IsEncrypted)[^0x14..];
            Hash = hash.Span.ToArray();

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
            Hash = archive.ReadArray<byte>(0x14).ToArray();

            if (owner.Version >= EPakVersion.CompressionEncryption) {
                if (CompressionMethod > 0) {
                    CompressionBlocks = archive.ReadArray<FPakCompressedBlock>().ToArray();
                }

                IsEncrypted = archive.Read<byte>() == 1;
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
    public byte[] Hash { get; }
    public FPakCompressedBlock[] CompressionBlocks { get; } = Array.Empty<FPakCompressedBlock>();
    public bool IsEncrypted { get; }
    public uint CompressionBlockSize { get; }
    public string Path { get; internal set; } = "";

    public IVFSFile Owner { get; } = null!;
    public long Size { get; }
    public string MountedPath { get; internal set; } = "";
    public string ObjectPath { get; internal set; } = "";
    public ulong MountedHash { get; internal set; }
    public IPoliteDisposable? Data { get; set; }
    public bool Disposed { get; private set; }

    public MemoryOwner<byte> ReadFile() => Owner.ReadFile(this);

    public UAssetFile? ReadAsset() => Owner.ReadAsset(this);

    public UObject? ReadAssetExport(int index) => Owner.ReadAssetExport(this, index);

    public UObject?[] ReadAssetExports() => Owner.ReadAssetExports(this);

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
    public void CreateObjectPath() {
        // Engine/Content/Stuff -> /Engine/Stuff -- normalize Engine
        if (MountedPath.StartsWith("Engine/Content")) {
            ObjectPath = "/Engine" + MountedPath[14..];
            return;
        }

        // Engine/Plugins/Stuff -> /Plugins/Stuff -- normalize Plugins
        if (MountedPath.StartsWith("Engine/Plugins")) {
            ObjectPath = MountedPath[6..];
            return;
        }

        // Project/Content/Stuff/ -> Game/Content/Stuff -- strip project nname
        var index = MountedPath.IndexOf('/');
        if (index == -1) {
            return;
        }

        if (MountedPath[..index] != "Game") {
            ObjectPath = MountedPath[(index + 1)..];
        }

        // Content/Stuff/ -> Stuff -- strip Content directory
        if (ObjectPath.StartsWith("Content/")) {
            ObjectPath = ObjectPath[8..];
        }

        // Stuff.uasset -> Stuff -- strip package extensions
        var extPos = ObjectPath.LastIndexOf('.');
        if (extPos > -1) {
            var ext = ObjectPath[extPos..];
            if (ext is ".uasset" or ".umap") {
                ObjectPath = ObjectPath[..^ext.Length];
            }
        }

        // Stuff -> /Game/Stuff -- rebuild path
        ObjectPath = "/Game/" + ObjectPath;
    }
}
