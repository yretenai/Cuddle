using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cuddle.Core.VFS;
using DragonLib.Hash;
using DragonLib.Hash.Basis;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core.Structs.FileSystem;

public class FIoDirectory {
    public FIoDirectory(FArchiveReader archive, HashPathStore hashStore, FIoStore owner) {
        Owner = owner;

        MountPoint = archive.ReadString();
        OriginalMountPoint = MountPoint;
        if (MountPoint.StartsWith("../../../")) {
            MountPoint = MountPoint[8..];
        }

        if (MountPoint == "None") {
            MountPoint = "";
        }

        if (MountPoint.Length > 1) {
            if (MountPoint.StartsWith("/")) {
                MountPoint = MountPoint[1..];
            }

            if (!MountPoint.EndsWith("/")) {
                MountPoint += '/';
            }
        }

        using var crc = CyclicRedundancyCheck.Create(CRC32Variants.ISO);
        var pathHashSeed = crc.ComputeHashValue(hashStore.Encoding.GetBytes(owner.Name.ToLower()));
        DirectoryEntries = archive.ReadArray<FIoDirectoryIndexEntry>().ToArray();
        FileEntries = archive.ReadArray<FIoFileIndexEntry>().ToArray();
        StringTable = archive.ReadStrings();

        Files.EnsureCapacity(FileEntries.Length);

        void EnumerateFiles(string root, uint fileId) {
            while (fileId != uint.MaxValue) {
                var fileEntry = FileEntries[fileId];
                var path = string.Concat(root, StringTable[fileEntry.NameIndex]);
                var mountedPath = MountPoint + path;
                // todo: calc hash.
                var tocEntry = Owner.Toc.ChunkOffsetLengths.Span[fileEntry.UserData];
                Files.Add(new FIoFile(mountedPath,
                    FPakEntry.CreateObjectPath(mountedPath),
                    hashStore?.AddPath(path, pathHashSeed, false) ?? 0,
                    tocEntry.Length,
                    tocEntry.Offset,
                    fileEntry.UserData,
                    Owner));

                fileId = fileEntry.NextFileIndex;
            }
        }

        void EnumerateDirectories(string root, uint dirId) {
            while (dirId < uint.MaxValue) {
                var (nameIndex, firstChildIndex, nextSiblingIndex, fileId) = DirectoryEntries[dirId];
                var dirPath = nameIndex == uint.MaxValue ? root : $"{root}{StringTable[nameIndex]}/";
                EnumerateFiles(dirPath, fileId);
                EnumerateDirectories(dirPath, firstChildIndex);
                dirId = nextSiblingIndex;
            }
        }

        EnumerateDirectories(string.Empty, 0);
    }

    public string MountPoint { get; set; }
    public string OriginalMountPoint { get; set; }
    public FIoDirectoryIndexEntry[] DirectoryEntries { get; set; }
    public FIoFileIndexEntry[] FileEntries { get; set; }
    public string[] StringTable { get; set; }
    public List<FIoFile> Files { get; set; } = new();
    public FIoStore Owner { get; set; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
public record struct FIoDirectoryIndexEntry(uint NameIndex, uint FirstChildIndex, uint NextSiblingIndex, uint FirstFileIndex);

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
public record struct FIoFileIndexEntry(uint NameIndex, uint NextFileIndex, int UserData);

public sealed record FIoFile(string MountedPath, string ObjectPath, ulong MountedHash, long Size, long Offset, int Index, IVFSFile Owner) : IVFSEntry {
    public bool Disposed { get; set; }
    public IPoliteDisposable? Data { get; set; }

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

    ~FIoFile() {
        Dispose();
    }
}
