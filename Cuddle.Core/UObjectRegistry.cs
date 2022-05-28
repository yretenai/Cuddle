using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core;

public static class UObjectRegistry {
    public static UObject? Create(string? className, FObjectExport export, UAssetFile uasset) {
        using var data = uasset.ExportData.Partition((int) export.SerialOffset, (int) export.SerialSize);

        if (string.IsNullOrEmpty(className)) {
            className = "Object";
        }

        // todo: load and find uobject implementations.

        try {
            return new UObject(data);
        } catch {
            return null;
        }
    }
}
