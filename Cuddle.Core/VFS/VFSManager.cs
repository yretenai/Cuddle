﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cuddle.Core.Assets;
using Cuddle.Core.Structs;
using Cuddle.Security;
using DragonLib.Text;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Serilog;

namespace Cuddle.Core.VFS;

public sealed class VFSManager : IResettable {
    public VFSManager() => Culture = new CultureStore(this);

    public AESKeyStore KeyStore { get; } = new();
    public HashPathStore HashStore { get; } = new();
    public CultureStore Culture { get; set; }
    public Oodle? Oodle { get; set; }

    public List<IVFSFile> Containers { get; } = new();
    public IVFSEntry[] Files { get; set; } = Array.Empty<IVFSEntry>();
    public Dictionary<string, IVFSEntry> UniqueFilesPath { get; private set; } = new();
    public Dictionary<string, IVFSEntry> UniqueFilesObjectPath { get; private set; } = new();
    public Dictionary<ulong, IVFSEntry> UniqueFilesHash { get; private set; } = new();
    public bool IsCaseInsensitive { get; private set; }

    public bool Disposed { get; private set; }

    public void Dispose() {
        foreach (var vfs in Containers) {
            vfs.Dispose();
        }

        Containers.Clear();

        Oodle?.Dispose();
        Oodle = null;

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

        var global = dir.GetFiles("global.ucas", SearchOption.AllDirectories).FirstOrDefault();
        var found = new HashSet<string>();
        if (global is not null) {
            found.Add("global");
            Containers.Add(new FIoStore(global.FullName, game, Path.GetFileNameWithoutExtension(global.Name), this) { IsGlobal = true });
        }

        foreach (var casPath in dir.EnumerateFiles("*.ucas", SearchOption.TopDirectoryOnly).OrderBy(x => x.Name.Replace('.', '_'), new NaturalStringComparer(StringComparison.OrdinalIgnoreCase))) {
            var name = Path.GetFileNameWithoutExtension(casPath.Name);
            if (found.Add(name)) {
                Containers.Add(new FIoStore(casPath.FullName, game, name, this));
            }
        }

        foreach (var pakPath in dir.EnumerateFiles("*.pak", SearchOption.AllDirectories).OrderBy(x => x.Name.Replace('.', '_'), new NaturalStringComparer(StringComparison.OrdinalIgnoreCase))) {
            var name = Path.GetFileNameWithoutExtension(pakPath.Name);
            if (found.Add(name)) {
                Containers.Add(new FPakFile(pakPath.FullName, game, name, this));
            }
        }
    }

    private IVFSEntry? TryFindFile(string path, out FName name) {
        name = FName.Null;
        var isObject = path.StartsWith("/Game/", IsCaseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.Ordinal);

        if (isObject && UniqueFilesObjectPath.TryGetValue(path, out var file)) {
            return file;
        }

        if (UniqueFilesPath.TryGetValue(path, out file)) {
            return file;
        }

        var ext = Path.GetExtension(path);
        if (ext.Length <= 1) {
            return null;
        }

        name = new FName(ext[1..]);
        var strippedPath = path[..path.IndexOf('.', StringComparison.Ordinal)];

        if (isObject && UniqueFilesObjectPath.TryGetValue(strippedPath, out file)) {
            return file;
        }

        return UniqueFilesPath.TryGetValue(strippedPath, out file) ? file : null;
    }

    public MemoryOwner<byte> ReadFile(string path) {
        var file = TryFindFile(path, out _);
        return file == null ? MemoryOwner<byte>.Empty : file.Owner.ReadFile(file);
    }

    public MemoryOwner<byte> ReadFile(ulong hash) => !UniqueFilesHash.TryGetValue(hash, out var file) ? MemoryOwner<byte>.Empty : file.Owner.ReadFile(file);

    public UAssetFile? ReadAsset(IVFSEntry? entry) {
        if (entry == null || Disposed || entry.Owner.Disposed) {
            return null;
        }

        if (entry.Disposed) {
            entry.Reset();
        }

        if (entry.Data is null) {
            var data = entry.ReadFile();
            if (data.Length == 0) {
                return null;
            }

            var uexp = ReadFile(Path.ChangeExtension(entry.MountedPath, ".uexp"));
            entry.Data = new UAssetFile(data, uexp, Path.GetFileNameWithoutExtension(entry.MountedPath), entry.Owner.Game, entry.Owner, this);
        }

        return (UAssetFile) entry.Data;
    }

    public UAssetFile? ReadAsset(string path, out FName name) {
        var file = TryFindFile(path, out name);
        return ReadAsset(file);
    }

    public UAssetFile? ReadAsset(ulong hash) {
        UniqueFilesHash.TryGetValue(hash, out var file);
        return ReadAsset(file);
    }

    public UObject? ReadExport(IVFSEntry entry, int index) => ReadAsset(entry)?.GetExport(index);
    public UObject? ReadExport(string path, int index) => ReadAsset(path, out _)?.GetExport(index);

    public UObject? ReadExport(string path) {
        var asset = ReadAsset(path, out var name);
        return asset?.GetExport(name);
    }

    public UObject? ReadExport(FName path) {
        var asset = ReadAsset(path.Value, out var name);
        return asset?.GetExport(name with { Instance = path.Instance });
    }

    public UObject? ReadExport(ulong hash, int index) => ReadAsset(hash)?.GetExport(index);
    public UObject?[] ReadExports(IVFSEntry entry) => ReadAsset(entry)?.GetExports() ?? Array.Empty<UObject>();
    public UObject?[] ReadExports(string path) => ReadAsset(path, out _)?.GetExports() ?? Array.Empty<UObject>();
    public UObject?[] ReadExports(ulong hash) => ReadAsset(hash)?.GetExports() ?? Array.Empty<UObject>();

    public void Freeze(bool caseInsensitive = false) {
        IsCaseInsensitive = caseInsensitive;
        Files = Containers.SelectMany(x => x.Entries).ToArray();
        UniqueFilesPath = Files.DistinctBy(x => caseInsensitive ? x.MountedPath.ToLower() : x.MountedPath).ToDictionary(x => caseInsensitive ? x.MountedPath.ToLower() : x.MountedPath, IsCaseInsensitive ? StringComparer.InvariantCultureIgnoreCase : StringComparer.Ordinal);
        UniqueFilesObjectPath = Files.DistinctBy(x =>  caseInsensitive ? x.ObjectPath.ToLower() : x.ObjectPath).ToDictionary(x => caseInsensitive ? x.ObjectPath.ToLower() : x.ObjectPath, IsCaseInsensitive ? StringComparer.InvariantCultureIgnoreCase : StringComparer.Ordinal);
        UniqueFilesHash = Files.DistinctBy(x => x.MountedHash).ToDictionary(x => x.MountedHash);
    }
}
