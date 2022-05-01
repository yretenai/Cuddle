using Cuddle.Core.FileSystem;

namespace Cuddle.Core.Structs.Asset;

public class FObjectImport {
    public FObjectImport() { }

    public FObjectImport(FArchive archive) {
        ClassPackage = new FName(archive);
        ClassName = new FName(archive);
        PackageRef = new FPackageIndex(archive);
        ObjectName = new FName(archive);
    }

    public FName ClassPackage { get; } = FName.Null;
    public FName ClassName { get; } = FName.Null;
    public FPackageIndex PackageRef { get; } = FPackageIndex.Null;
    public FName ObjectName { get; } = FName.Null;
}
