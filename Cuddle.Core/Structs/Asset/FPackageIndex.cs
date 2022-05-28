using System.Diagnostics;
using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Asset;

public class FPackageIndex {
    public FPackageIndex() { }

    public FPackageIndex(FArchiveReader archive) {
        Index = archive.Read<int>();
        Asset = archive.Asset;
    }

    private UAssetFile? Asset { get; }
    public int Index { get; init; }

    public FObjectAbstract? Reference {
        get {
            if (Asset == null) {
                return null;
            }

            var importIndex = 0 - Index - 1;
            var exportIndex = Index - 1;
            if (IsImport && Asset.Imports.Length > importIndex) {
                return Asset.Imports[importIndex];
            }

            if (IsExport && Asset.Exports.Length > exportIndex) {
                return Asset.Exports[exportIndex];
            }

            return null;
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public UObject? Object => Asset?.GetIndex(this);

    public bool IsImport => Index < 0;
    public bool IsExport => Index > 0;
    public bool IsNull => Index == 0;

    public static FPackageIndex Null { get; } = new() { Index = 0 };
}
