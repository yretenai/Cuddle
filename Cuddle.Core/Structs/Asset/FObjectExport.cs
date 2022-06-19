using System;
using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Asset;

public sealed class FObjectExport : FObjectAbstract, IResettable {
    public FObjectExport() { }

    public FObjectExport(FArchiveReader archive) {
        ClassIndex = new FPackageIndex(archive);
        SuperIndex = new FPackageIndex(archive);
        if (archive.Version >= EObjectVersion.TemplateIndex_IN_COOKED_EXPORTS) {
            TemplateIndex = new FPackageIndex(archive);
        }

        OuterIndex = new FPackageIndex(archive);
        ObjectName = new FName(archive);
        ObjectFlags = archive.Read<EObjectFlags>();
        SerialSize = archive.Version >= EObjectVersion.LONG_EXPORTMAP_SERIALSIZES ? archive.Read<long>() : archive.Read<int>();
        SerialOffset = archive.Version >= EObjectVersion.LONG_EXPORTMAP_SERIALSIZES ? archive.Read<long>() : archive.Read<int>();
        ForcedExport = archive.ReadBoolean();
        NotForClient = archive.ReadBoolean();
        NotForServer = archive.ReadBoolean();
        PackageGuid = archive.Read<Guid>();
        PackageFlags = archive.Read<uint>();
        NotAlwaysLoadedForEditorGame = archive.Version < EObjectVersion.LOAD_FOR_EDITOR_GAME || archive.ReadBoolean();
        IsAsset = archive.Version >= EObjectVersion.COOKED_ASSETS_IN_EDITOR_SUPPORT && archive.ReadBoolean();

        if (archive.Version >= EObjectVersion.PRELOAD_DEPENDENCIES_IN_COOKED_EXPORTS) {
            FirstExportDependency = archive.Read<int>();
            SerializationBeforeSerializationDependencies = archive.Read<int>();
            CreateBeforeSerializationDependencies = archive.Read<int>();
            SerializationBeforeCreateDependencies = archive.Read<int>();
            CreateBeforeCreateDependencies = archive.Read<int>();
        }
    }

    public FPackageIndex ClassIndex { get; } = FPackageIndex.Null;
    public FPackageIndex SuperIndex { get; } = FPackageIndex.Null;
    public FPackageIndex TemplateIndex { get; } = FPackageIndex.Null;
    public FPackageIndex OuterIndex { get; } = FPackageIndex.Null;
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
    public int SerializationBeforeSerializationDependencies { get; }
    public int CreateBeforeSerializationDependencies { get; }
    public int SerializationBeforeCreateDependencies { get; }
    public int CreateBeforeCreateDependencies { get; }
    public UObject? Object { get; internal set; }
    public bool ObjectCreated { get; internal set; }
    public bool Disposed { get; private set; }

    public void Dispose() {
        if (Object is IDisposable disposable) {
            disposable.Dispose();
        }

        Object = null;

        ObjectCreated = false;

        if (Disposed) {
            return;
        }

        GC.SuppressFinalize(this);
        Disposed = true;
    }

    public void Reset() {
        if (Object is IDisposable disposable) {
            disposable.Dispose();
        }

        Object = null;

        ObjectCreated = false;

        if (Disposed) {
            GC.ReRegisterForFinalize(this);
            Disposed = false;
        }
    }

    ~FObjectExport() {
        Dispose();
    }
}
