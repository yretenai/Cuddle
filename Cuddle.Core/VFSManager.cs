using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cuddle.Core.Enums;
using DragonLib.Text;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core;

public sealed class VFSManager : IDisposable {
    public AESKeyStore KeyStore { get; } = new();
    public HashPathStore HashStore { get; } = new();

    public List<IVFSFile> Containers { get; } = new();
    public IEnumerable<IVFSEntry> Files => Containers.SelectMany(x => x.Entries);
    public IEnumerable<IVFSEntry> UniqueFilesPath => Files.DistinctBy(x => x.MountedPath);
    public IEnumerable<IVFSEntry> UniqueFilesHash => Files.DistinctBy(x => x.MountedPathHash);

    public bool Disposed { get; private set; }

    public void Dispose() {
        foreach (var vfs in Containers) {
            vfs.Dispose();
        }

        Containers.Clear();

        if (Disposed) {
            return;
        }

        GC.SuppressFinalize(this);
        Disposed = true;
    }

    ~VFSManager() {
        Dispose();
    }

    public void MountPakDir(DirectoryInfo dir, EGame game) {
        foreach (var pakPath in dir.GetFiles("*.pak", SearchOption.TopDirectoryOnly).OrderBy(x => x.Name.Replace('.', '_'), new NaturalStringComparer(StringComparison.OrdinalIgnoreCase, true))) {
            Containers.Add(new UPakFile(pakPath.FullName, game, Path.GetFileNameWithoutExtension(pakPath.Name), KeyStore, HashStore));
        }
    }

    public MemoryOwner<byte> ReadFile(string path) {
        var file = Files.FirstOrDefault(x => x.MountedPath.Equals(path, StringComparison.Ordinal));
        return file == null ? MemoryOwner<byte>.Empty : file.Owner.ReadFile(file);
    }

    public UObject? ReadExport(string path, int index) {
        var file = Files.FirstOrDefault(x => x.MountedPath.Equals(path, StringComparison.Ordinal));
        return file?.Owner.ReadAssetExport(file, index);
    }

    public UObject?[] ReadExports(string path) {
        var file = Files.FirstOrDefault(x => x.MountedPath.Equals(path, StringComparison.Ordinal));
        return file == null ? Array.Empty<UObject>() : file.Owner.ReadAssetExports(file);
    }
}
