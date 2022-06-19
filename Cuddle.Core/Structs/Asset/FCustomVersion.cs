using System;

namespace Cuddle.Core.Structs.Asset;

// https://github.com/gildor2/UEViewer/blob/9902e299bdc2e1ecc6e8fd26859f1def18f89ced/Unreal/UnrealPackage/UnPackage4.cpp#L12-L54
public record struct FCustomVersion {
    public FCustomVersion() {
        Key = Guid.Empty;
        Version = 0;
    }

    public FCustomVersion(FGuidCustomVersion guidCustomVersion) {
        Key = guidCustomVersion.Key;
        Version = guidCustomVersion.Version;
    }

    public FCustomVersion(FEnumCustomVersion enumCustomVersion) {
        Key = Guid.Empty;
        Version = enumCustomVersion.Version;
    }

    public Guid Key { get; }
    public int Version { get; }
}
