using Cuddle.Core.FileSystem;

namespace Cuddle.Core.Structs.Asset;

public class FObjectImport : FObjectAbstract {
    public FObjectImport() { }

    public FObjectImport(FArchive archive) {
        ClassPackage = new FName(archive);
        ClassName = new FName(archive);
        PackageIndex = new FPackageIndex(archive);
        ObjectName = new FName(archive);
    }

    public FName ClassPackage { get; } = FName.Null;
    public FName ClassName { get; } = FName.Null;
    public FPackageIndex PackageIndex { get; protected init; } = FPackageIndex.Null;
}
