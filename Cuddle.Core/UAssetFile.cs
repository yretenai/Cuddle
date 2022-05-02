using System;
using Cuddle.Core.Enums;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core;

public class UAssetFile {
    public UAssetFile(ReadOnlyMemory<byte> uasset, string name, EGame game) {
        Game = game;
        Name = name;

        var archive = new FArchive(this, uasset);
        Summary = new FPackageFileSummary(archive);

        archive.Position = Summary.NameOffset;
        Names = archive.ReadClassArray<FNameEntry>(Summary.NameCount);

        archive.Position = Summary.ImportOffset;
        Imports = archive.ReadClassArray<FObjectImport>(Summary.ImportCount);

        archive.Position = Summary.ExportOffset;
        Exports = archive.ReadClassArray<FObjectExport>(Summary.ExportCount);
    }

    public EGame Game { get; }

    public string Name { get; }
    public FPackageFileSummary Summary { get; }
    public FNameEntry[] Names { get; }
    public FObjectImport[] Imports { get; }
    public FObjectExport[] Exports { get; }
}
