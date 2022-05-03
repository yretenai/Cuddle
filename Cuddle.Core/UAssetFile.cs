using System;
using Cuddle.Core.Enums;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core;

public class UAssetFile {
    public UAssetFile(ReadOnlyMemory<byte> uasset, ReadOnlyMemory<byte> uexp, ReadOnlyMemory<byte> ubulk, ReadOnlyMemory<byte> uptnl, string name, EGame game, UPakFile? owner) {
        Game = game;
        Name = name;
        Owner = owner;

        var archive = new FArchiveReader(this, uasset);
        Summary = new FPackageFileSummary(archive);

        archive.Position = Summary.NameOffset;
        Names = archive.ReadClassArray<FNameEntry>(Summary.NameCount);

        archive.Position = Summary.ImportOffset;
        Imports = archive.ReadClassArray<FObjectImport>(Summary.ImportCount);

        archive.Position = Summary.ExportOffset;
        Exports = archive.ReadClassArray<FObjectExport>(Summary.ExportCount);

        var combined = new byte[uasset.Length + uexp.Length].AsMemory();
        uasset.CopyTo(combined);
        uexp.CopyTo(combined[Summary.TotalHeaderSize..]);
        ExportData = new FArchiveReader(this, combined);
        BulkData = new FArchiveReader(this, ubulk);
        OptionalData = new FArchiveReader(this, uptnl);
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

    public UObject? GetExport(int index) {
        if (index > Exports.Length) {
            return null;
        }

        if (!Exports[index].ObjectCreated) {
            var export = Exports[index];
            if (export.SerialOffset > 0x7FFFFFF) {
                throw new IndexOutOfRangeException("Export is outside of reasonable range");
            }

            Exports[index].Object = UObjectRegistry.Create(export.ClassIndex.Reference?.ObjectName, export, this);
            Exports[index].ObjectCreated = true;
        }

        return Exports[index].Object;
    }

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
}
