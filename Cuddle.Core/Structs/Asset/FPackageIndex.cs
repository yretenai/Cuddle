using Cuddle.Core.FileSystem;

namespace Cuddle.Core.Structs.Asset;

public class FPackageIndex {
    public FPackageIndex() { }

    public FPackageIndex(FArchive archive) {
        Index = archive.Read<int>();
        var importIndex = 0 - Index - 1;
        var exportIndex = Index - 1;
        if (IsImport && archive.Asset.Imports.Length > importIndex) {
            ResolvedObject = archive.Asset.Imports[importIndex];
        }

        if (IsExport && archive.Asset.Exports.Length > exportIndex) {
            ResolvedObject = archive.Asset.Exports[exportIndex];
        }
    }

    public int Index { get; init; }

    public object? ResolvedObject { get; }

    public bool IsImport => Index < 0;
    public bool IsExport => Index > 0;
    public bool IsNull => Index == 0;

    public string Name =>
        ResolvedObject switch {
            FObjectExport export => export.ObjectName,
            FObjectImport import => import.ObjectName,
            _ => "None",
        };

    public string FullName =>
        ResolvedObject switch {
            FObjectExport export => export.ObjectName,
            FObjectImport import => import.PackageRef.IsNull ? import.ObjectName : import.PackageRef.Name,
            _ => "None",
        };

    public static FPackageIndex Null { get; } = new() { Index = 0 };
}
