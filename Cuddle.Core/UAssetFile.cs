using System;
using System.Linq;
using Cuddle.Core.Enums;
using Cuddle.Core.Structs.Asset;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core;

public class UAssetFile : IDisposable {
    public UAssetFile(MemoryOwner<byte> uasset, MemoryOwner<byte> uexp, MemoryOwner<byte> ubulk, MemoryOwner<byte> uptnl, string name, EGame game, UPakFile? owner) {
        Game = game;
        Name = name;
        Owner = owner;

        using var archive = new FArchiveReader(game, uasset);
        Summary = new FPackageFileSummary(archive);
        archive.Asset = this;
        archive.Version = Summary.FileVersionUE4;

        archive.Position = Summary.NameOffset;
        Names = archive.ReadClassArray<FNameEntry>(Summary.NameCount);

        archive.Position = Summary.ImportOffset;
        Imports = archive.ReadClassArray<FObjectImport>(Summary.ImportCount);

        archive.Position = Summary.ExportOffset;
        Exports = archive.ReadClassArray<FObjectExport>(Summary.ExportCount);

        var combined = MemoryOwner<byte>.Allocate(uasset.Length + uexp.Length);
        uasset.Memory.CopyTo(combined.Memory);
        uexp.Memory.CopyTo(combined.Memory[Summary.TotalHeaderSize..]);
        ExportData = new FArchiveReader(this, combined);
        BulkData = new FArchiveReader(this, ubulk);
        OptionalData = new FArchiveReader(this, uptnl);
        
        uasset.Dispose();
        uexp.Dispose();
    }
    
    ~UAssetFile() {
        Dispose();
    }

    public UPakFile? Owner { get; }
    public EGame Game { get; }
    public string Name { get; }
    public FPackageFileSummary Summary { get; }
    public FNameEntry[] Names { get; }
    public FObjectImport[] Imports { get; }
    public FObjectExport[] Exports { get; }
    public FArchiveReader ExportData { get; }
    public FArchiveReader BulkData { get; }
    public FArchiveReader OptionalData { get; }

    public UObject? GetExport(int index) => index > Exports.Length ? null : GetExport(Exports[index]);

    private UObject? GetExport(FObjectExport export) {
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

        if (Imports[index].ObjectCreated) {
            return Imports[index].Object;
        }

        throw new NotImplementedException();
    }

    public UObject? GetIndex(FPackageIndex index) {
        if (Owner == null || index.IsNull) {
            return null;
        }

        return index.IsExport ? GetExport(index.Index - 1) : GetImport(0 - index.Index - 1);
    }
    
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        ExportData.Dispose();
        BulkData.Dispose();
        OptionalData.Dispose();
        
        if (Disposed) {
            return;
        }

        GC.SuppressFinalize(this);
        Disposed = true;
    }
}
