using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core.Assets;

public sealed class UAssetFile : IPoliteDisposable {
    public UAssetFile(MemoryOwner<byte> uasset, MemoryOwner<byte> uexp, string name, EGame game, IVFSFile? owner, VFSManager manager) {
        Game = game;
        Name = name;
        Owner = owner;
        Manager = manager;

        using var archive = new FArchiveReader(game, uasset, manager);
        Summary = new FPackageFileSummary(archive, Name);
        archive.Asset = this;
        archive.Version = Summary.FileVersionUE4;
        archive.VersionUE5 = Summary.FileVersionUE5;

        archive.Position = Summary.NameOffset;
        Names = archive.ReadClassArray<FNameEntry>(Summary.NameCount);

        archive.Position = Summary.ImportOffset;
        Imports = archive.ReadClassArray<FObjectImport>(Summary.ImportCount);

        archive.Position = Summary.ExportOffset;
        Exports = archive.ReadClassArray<FObjectExport>(Summary.ExportCount);

        if (Summary.DependsOffset > 0) {
            Dependencies = new FPackageIndex[Summary.ExportCount][];
            archive.Position = Summary.DependsOffset;
            for (var i = 0; i < Summary.ExportCount; i++) {
                Dependencies[i] = archive.ReadClassArray<FPackageIndex>();
            }
        } else {
            Dependencies = Array.Empty<FPackageIndex[]>();
        }

        if (Summary.PreloadDependencyOffset > 0) {
            archive.Position = Summary.PreloadDependencyOffset;
            Preload = archive.ReadClassArray<FPackageIndex>(Summary.PreloadDependencyCount);
        } else {
            Preload = Array.Empty<FPackageIndex>();
        }

        // todo: gatherable name.

        if (Summary.SoftPackageReferencesOffset > 0) {
            archive.Position = Summary.SoftPackageReferencesOffset;
            PackageReferences = Summary.FileVersionUE4 >= EObjectVersion.ADDED_SOFT_OBJECT_PATH ? archive.ReadClassArray<FSoftObjectPath>(Summary.SoftPackageReferencesCount) : archive.ReadStrings(Summary.SoftPackageReferencesCount).Select(x => new FSoftObjectPath(x)).ToArray();
        } else {
            PackageReferences = Array.Empty<FSoftObjectPath>();
        }

        if (Summary.SearchableNamesOffset > 0) {
            archive.Position = Summary.SearchableNamesOffset;
            var count = archive.Read<int>();
            SearchableNames = new Dictionary<FPackageIndex, FName[]>(count);
            for (var i = 0; i < count; ++i) {
                SearchableNames.Add(new FPackageIndex(archive), archive.ReadNames());
            }
        } else {
            SearchableNames = new Dictionary<FPackageIndex, FName[]>();
        }

        if (Summary.ThumbnailTableOffset > 0) {
            archive.Position = Summary.ThumbnailTableOffset;
            ThumbnailTable = archive.ReadClassArray<FThumbnailTableEntry>();
        } else {
            ThumbnailTable = Array.Empty<FThumbnailTableEntry>();
        }

        if (Summary.WorldTileInfoDataOffset > 0) {
            WorldTileInfo = new FWorldTileInfo(archive);
        }

        if (Summary.PayloadTocOffset > 0) {
            Payload = new FPackageTrailer(archive);
        }

        var combined = MemoryOwner<byte>.Allocate(uasset.Length + uexp.Length);
        uasset.Memory.CopyTo(combined.Memory);
        uexp.Memory.CopyTo(combined.Memory[Summary.TotalHeaderSize..]);
        ExportData = new FArchiveReader(this, combined);
        uasset.Dispose();
        uexp.Dispose();
    }

    public IVFSFile? Owner { get; }
    public VFSManager Manager { get; }
    public EGame Game { get; }
    public string Name { get; }
    public FPackageFileSummary Summary { get; }
    public FNameEntry[] Names { get; }
    public FObjectImport[] Imports { get; }
    public FObjectExport[] Exports { get; }
    public FPackageIndex[][] Dependencies { get; }
    public FPackageIndex[] Preload { get; }
    public FSoftObjectPath[] PackageReferences { get; }
    public Dictionary<FPackageIndex, FName[]> SearchableNames { get; }
    public FThumbnailTableEntry[] ThumbnailTable { get; }
    public FArchiveReader ExportData { get; }
    public FWorldTileInfo? WorldTileInfo { get; }
    public FPackageTrailer? Payload { get; }

    public bool Disposed { get; private set; }

    public void Dispose() {
        ExportData.Dispose();

        foreach (var export in Exports) {
            export.Dispose();
        }

        if (Disposed) {
            return;
        }

        GC.SuppressFinalize(this);
        Disposed = true;
    }

    ~UAssetFile() {
        Dispose();
    }

    public UObject? GetExport(int index) => index > Exports.Length ? null : GetExport(Exports[index]);
    public UObject? GetExport(FName name) => GetExport(Exports.FirstOrDefault(x => x.ObjectName.Equals(name)));

    private UObject? GetExport(FObjectExport? export) {
        if (export == null) {
            return null;
        }

        if (export.Disposed) {
            export.Reset();
        }

        if (Summary.PackageFlags.HasFlag(EPackageFlags.UnversionedProperties)) {
            throw new NotImplementedException();
        }

        if (!export.ObjectCreated) {
            if (export.SerialOffset > 0x7FFFFFF) {
                throw new IndexOutOfRangeException("Export is outside of reasonable range");
            }

            export.Object = UObjectRegistry.Create(export.ClassIndex.Reference?.ObjectName, export, this);
            export.ObjectCreated = true;
        }

        return export.Object;
    }

    public UObject?[] GetExports() => Exports.Select(GetExport).ToArray();

    public UObject? GetImport(int index) {
        if (Owner == null || index > Imports.Length) {
            return null;
        }

        var import = Imports[index];
        if (import.ObjectCreated) {
            return import.Object;
        }

        var outer = import;
        while (!outer.PackageIndex.IsNull) {
            outer = Imports[-outer.PackageIndex.Index - 1];
        }

        import.ObjectCreated = true;

        if (outer.ObjectName.Value.StartsWith("/Script/")) {
            // not supported.
            return null;
        }

        if (outer.ObjectName.Value == "None") {
            // package reference is weird.
            return null;
        }

        import.Object = Owner.Manager.ReadExport(outer.ObjectName);
        import.ObjectCreated = true;
        return import.Object;
    }

    public UObject? GetIndex(FPackageIndex index) {
        if (Owner == null || index.IsNull) {
            return null;
        }

        return index.IsExport ? GetExport(index.Index - 1) : GetImport(0 - index.Index - 1);
    }

    public static string? GetFullPath(FObjectAbstract? reference) {
        switch (reference) {
            case FObjectExport export: {
                if (export.OuterIndex.IsNull || export.OuterIndex.Reference == reference) {
                    return reference.ObjectName;
                }

                var path = GetFullPath(export.OuterIndex.Reference);
                if (path.IsNullOrNone()) {
                    return reference.ObjectName;
                }

                return path + "." + reference.ObjectName;
            }
            case FObjectImport import: {
                if (import.PackageIndex.IsNull || import.PackageIndex.Reference == reference) {
                    return reference.ObjectName;
                }

                var path = GetFullPath(import.PackageIndex.Reference);
                if (path.IsNullOrNone()) {
                    return reference.ObjectName;
                }

                return path + "." + reference.ObjectName;
            }
            default:
                return null;
        }
    }

    public bool TryOpenBulk([MaybeNullWhen(false)] out FArchiveReader reader) {
        reader = null;

        var bulk = Manager.ReadFile(Name + ".ubulk");
        if (bulk.Length == 0) {
            bulk.Dispose();
            return false;
        }

        reader = new FArchiveReader(this, bulk);
        return true;
    }

    public bool TryOpenOptional([MaybeNullWhen(false)] out FArchiveReader reader) {
        reader = null;

        var ptnl = Manager.ReadFile(Name + ".uptnl");
        if (ptnl.Length == 0) {
            ptnl.Dispose();
            return false;
        }

        reader = new FArchiveReader(this, ptnl);
        return true;
    }
}
