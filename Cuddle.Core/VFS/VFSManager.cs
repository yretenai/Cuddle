using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cuddle.Core.Assets;
using Cuddle.Core.Structs;
using DragonLib.Text;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Serilog;

namespace Cuddle.Core.VFS;

public sealed class VFSManager : IResettable {
    public AESKeyStore KeyStore { get; } = new();
    public HashPathStore HashStore { get; } = new();

    public List<IVFSFile> Containers { get; } = new();
    public IEnumerable<IVFSEntry> Files => Containers.SelectMany(x => x.Entries);
    public IEnumerable<IVFSEntry> UniqueFilesPath => Files.DistinctBy(x => x.MountedPath);
    public IEnumerable<IVFSEntry> UniqueFilesHash => Files.DistinctBy(x => x.MountedHash);

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

    public void Reset() {
        foreach (var vfs in Containers) {
            vfs.Dispose();
        }

        Containers.Clear();

        if (Disposed) {
            GC.ReRegisterForFinalize(this);
            Disposed = false;
        }
    }

    ~VFSManager() {
        Dispose();
    }

    public void MountDir(DirectoryInfo dir, EGame game) {
        Log.Information("Loading directory {Directory} with game {Game}", dir.Name, game.AsFormattedString());
        // this natural language sort is an easy hack to get pak ordering correctly.
        var keyFile = dir.EnumerateFiles("key.txt", SearchOption.AllDirectories).FirstOrDefault();
        var gameFile = dir.EnumerateFiles("game.txt", SearchOption.AllDirectories).FirstOrDefault();
        if (keyFile != null) {
            Log.Information("Found key file, attempting to parse...");
            foreach (var key in File.ReadAllLines(keyFile.FullName)) {
                KeyStore.AddKey(key);
            }
        }

        KeyStore.Dump();

        if (gameFile != null) {
            Log.Information("Found game file, attempting to parse...");
            if (Enum.TryParse<EGame>(File.ReadAllText(gameFile.FullName), out var tmpGame)) {
                Log.Information("  {Game} -> {NewGame}", game, tmpGame);
                game = tmpGame;
            }
        }

        foreach (var pakPath in dir.EnumerateFiles("*.pak", SearchOption.AllDirectories).OrderBy(x => x.Name.Replace('.', '_'), new NaturalStringComparer(StringComparison.OrdinalIgnoreCase))) {
            Containers.Add(new UPakFile(pakPath.FullName, game, Path.GetFileNameWithoutExtension(pakPath.Name), KeyStore, HashStore, this));
        }
    }

    public MemoryOwner<byte> ReadFile(string path) {
        var file = Files.FirstOrDefault(x => x.MountedPath.Equals(path, StringComparison.Ordinal) || x.ObjectPath.Equals(path, StringComparison.Ordinal));
        return file == null ? MemoryOwner<byte>.Empty : file.Owner.ReadFile(file);
    }

    public MemoryOwner<byte> ReadFile(ulong hash) {
        var file = Files.FirstOrDefault(x => x.MountedHash == hash);
        return file == null ? MemoryOwner<byte>.Empty : file.Owner.ReadFile(file);
    }

    public UAssetFile? ReadAsset(string path) {
        var file = Files.FirstOrDefault(x => x.MountedPath.Equals(path, StringComparison.Ordinal) || x.ObjectPath.Equals(path, StringComparison.Ordinal));
        return file?.Owner.ReadAsset(file);
    }

    public UAssetFile? ReadAsset(ulong hash) {
        var file = Files.FirstOrDefault(x => x.MountedHash == hash);
        return file?.Owner.ReadAsset(file);
    }

    public UObject? ReadExport(string path, int index) {
        var file = Files.FirstOrDefault(x => x.MountedPath.Equals(path, StringComparison.Ordinal) || x.ObjectPath.Equals(path, StringComparison.Ordinal));
        return file?.Owner.ReadAssetExport(file, index);
    }

    public UObject? ReadExport(ulong hash, int index) {
        var file = Files.FirstOrDefault(x => x.MountedHash == hash);
        return file?.Owner.ReadAssetExport(file, index);
    }

    public UObject?[] ReadExports(string path) {
        var file = Files.FirstOrDefault(x => x.MountedPath.Equals(path, StringComparison.Ordinal) || x.ObjectPath.Equals(path, StringComparison.Ordinal));
        return file == null ? Array.Empty<UObject>() : file.Owner.ReadAssetExports(file);
    }

    public UObject?[] ReadExports(ulong hash) {
        var file = Files.FirstOrDefault(x => x.MountedHash == hash);
        return file == null ? Array.Empty<UObject>() : file.Owner.ReadAssetExports(file);
    }
}
