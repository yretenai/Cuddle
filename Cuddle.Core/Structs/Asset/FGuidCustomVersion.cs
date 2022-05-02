using System;

namespace Cuddle.Core.Structs.Asset;

// https://github.com/gildor2/UEViewer/blob/9902e299bdc2e1ecc6e8fd26859f1def18f89ced/Unreal/UnrealPackage/UnPackage4.cpp#L12-L54
public class FGuidCustomVersion {
    public FGuidCustomVersion() { }

    public FGuidCustomVersion(FArchiveReader archive) {
        Key = archive.Read<Guid>();
        Version = archive.Read<int>();
        FriendlyName = archive.ReadString();
    }

    public Guid Key { get; }
    public int Version { get; }
    public string FriendlyName { get; } = "";
}
