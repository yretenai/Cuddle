using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cuddle.Core.Enums;
using Cuddle.Core.Structs.FileSystem;
using DragonLib.Text;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core;

public sealed class PakManager : IDisposable {
    public AESKeyStore KeyStore { get; } = new();
    public HashPathStore HashStore { get; } = new();

    public List<UPakFile> Paks { get; } = new();
    public IEnumerable<FPakEntry> Files => Paks.SelectMany(x => x.Index.Files);
    public IEnumerable<FPakEntry> UniqueFiles => Files.DistinctBy(x => x.MountedPath);

    public void Dispose() {
        foreach (var pak in Paks) {
            pak.Dispose();
        }

        Paks.Clear();
    }

    public void MountPakDir(DirectoryInfo dir, EGame game) {
        foreach (var pakPath in dir.GetFiles("*.pak", SearchOption.TopDirectoryOnly).OrderBy(x => x.Name.Replace('.', '_'), new NaturalStringComparer(StringComparison.OrdinalIgnoreCase, true))) {
            Paks.Add(new UPakFile(new FileStream(pakPath.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), game, Path.GetFileNameWithoutExtension(pakPath.Name), KeyStore, HashStore));
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
