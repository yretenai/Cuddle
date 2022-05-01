using System;
using Cuddle.Core.Enums;
using Cuddle.Core.FileSystem;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core;

public class UAssetFile {
    public UAssetFile(ReadOnlyMemory<byte> uasset, EGame game) {
        Game = game;

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

    public FPackageFileSummary Summary { get; set; }
    public FNameEntry[] Names { get; set; }
    public FObjectImport[] Imports { get; set; }
    public FObjectExport[] Exports { get; set; }
}
