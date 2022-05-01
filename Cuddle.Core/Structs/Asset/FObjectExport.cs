using System;
using Cuddle.Core.Enums;
using Cuddle.Core.FileSystem;

namespace Cuddle.Core.Structs.Asset;

public class FObjectExport {
    public FObjectExport() { }

    public FObjectExport(FArchive archive) {
        ClassIndex = new FPackageIndex(archive);
        SuperIndex = new FPackageIndex(archive);
        TemplateIndex = new FPackageIndex(archive);
        OuterIndex = new FPackageIndex(archive);
        ObjectName = new FName(archive);
        ObjectFlags = archive.Read<EObjectFlags>();
        SerialSize = archive.Asset.Summary.FileVersionUE4 >= EObjectVersion.LONG_EXPORTMAP_SERIALSIZES ? archive.Read<long>() : archive.Read<int>();
        SerialOffset = archive.Asset.Summary.FileVersionUE4 >= EObjectVersion.LONG_EXPORTMAP_SERIALSIZES ? archive.Read<long>() : archive.Read<int>();
        ForcedExport = archive.ReadBoolean();
        NotForClient = archive.ReadBoolean();
        NotForServer = archive.ReadBoolean();
        PackageGuid = archive.Read<Guid>();
        PackageFlags = archive.Read<uint>();
        NotAlwaysLoadedForEditorGame = archive.ReadBoolean();
        IsAsset = archive.ReadBoolean();
        FirstExportDependency = archive.Read<int>();
        SerializationBeforeSerializationDependencies = archive.ReadBoolean();
        CreateBeforeSerializationDependencies = archive.ReadBoolean();
        SerializationBeforeCreateDependencies = archive.ReadBoolean();
        CreateBeforeCreateDependencies = archive.ReadBoolean();
    }

    public FPackageIndex ClassIndex { get; } = FPackageIndex.Null;
    public FPackageIndex SuperIndex { get; } = FPackageIndex.Null;
    public FPackageIndex TemplateIndex { get; } = FPackageIndex.Null;
    public FPackageIndex OuterIndex { get; } = FPackageIndex.Null;
    public FName ObjectName { get; } = FName.Null;
    public EObjectFlags ObjectFlags { get; }
    public long SerialSize { get; }
    public long SerialOffset { get; }
    public bool ForcedExport { get; }
    public bool NotForClient { get; }
    public bool NotForServer { get; }
    public Guid PackageGuid { get; }
    public uint PackageFlags { get; }
    public bool NotAlwaysLoadedForEditorGame { get; }
    public bool IsAsset { get; }
    public int FirstExportDependency { get; }
    public bool SerializationBeforeSerializationDependencies { get; }
    public bool CreateBeforeSerializationDependencies { get; }
    public bool SerializationBeforeCreateDependencies { get; }
    public bool CreateBeforeCreateDependencies { get; }
}
