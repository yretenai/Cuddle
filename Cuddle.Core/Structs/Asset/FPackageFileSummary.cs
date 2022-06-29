using System;
using System.Linq;
using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Core.Structs.Asset;

// UE4 reference: FPackageFileSummary::Serialize (PackageFileSummary.h)
// https://github.com/gildor2/UEViewer/blob/60accbff70e58bfc66eaad4594416694b95422ee/Unreal/UnrealPackage/UnPackage4.cpp
public class FPackageFileSummary {
    public FPackageFileSummary() { }

    public FPackageFileSummary(FArchiveReader archive, string name) {
        Tag = archive.Read<uint>();
        if (Tag != 0x9E2A83C1) {
            Log.Error("Failed to read UAsset header for {Name}! Magic is invalid, expected 9E2A83C1 but got {Tag:X}", name, Tag);
            return;
        }

        LegacyFileVersion = archive.Read<ELegacyFileVersion>();

        LegacyUE3Version = LegacyFileVersion switch {
            > ELegacyFileVersion.OLDEST_LOADABLE_PACKAGE => throw new NotSupportedException(),
            < ELegacyFileVersion.NEWEST_LOADABLE_PACKAGE => throw new NotSupportedException(),
            ELegacyFileVersion.REMOVED_UE3_VERSION => LegacyUE3Version,
            _ => archive.Read<int>(),
        };

        FileVersionUE4 = archive.Read<EObjectVersion>();
        var isUnversioned = FileVersionUE4 == 0;
        if (FileVersionUE4 == 0) {
            // https://github.com/gildor2/UEViewer/blob/60accbff70e58bfc66eaad4594416694b95422ee/Unreal/UE4Version.h#L7
            FileVersionUE4 = archive.Game.FindObjectVersion();
        }

        if (FileVersionUE4 is > EObjectVersion.NEWEST_LOADABLE_PACKAGE or < EObjectVersion.OLDEST_LOADABLE_PACKAGE) {
            throw new NotSupportedException();
        }

        if (LegacyFileVersion <= ELegacyFileVersion.ADDED_UE5_VERSION) {
            FileVersionUE5 = (EObjectVersionUE5) archive.Read<int>();
        }

        if (FileVersionUE5 == 0 && archive.Game >= EGame.UE5_0) {
            FileVersionUE5 = archive.Game.FindObjectVersionUE5();
        }

        if (FileVersionUE5 > EObjectVersionUE5.NEWEST_LOADABLE_PACKAGE) {
            throw new NotSupportedException();
        }

        FileVersionLicenseeUE4 = archive.Read<int>();
        // https://github.com/gildor2/UEViewer/blob/9902e299bdc2e1ecc6e8fd26859f1def18f89ced/Unreal/UnrealPackage/UnPackage4.cpp#L12-L54
        CustomVersions = LegacyFileVersion switch {
            >= ELegacyFileVersion.CUSTOM_VERSION_ENUM => archive.ReadArray<FEnumCustomVersion>().ToArray().Select(x => new FCustomVersion(x)).ToArray(),
            >= ELegacyFileVersion.CUSTOM_VERSION_GUID => archive.ReadClassArray<FGuidCustomVersion>().Select(x => new FCustomVersion(x)).ToArray(),
            _ => archive.ReadArray<FCustomVersion>().ToArray(),
        };

        TotalHeaderSize = archive.Read<int>();
        FolderName = archive.ReadString();
        PackageFlags = archive.Read<EPackageFlags>();
        var hasEditorData = !(isUnversioned || PackageFlags.HasFlag(EPackageFlags.FilterEditorOnly));
        NameCount = archive.Read<int>();
        NameOffset = archive.Read<int>();
        if (FileVersionUE4 >= EObjectVersion.ADDED_PACKAGE_SUMMARY_LOCALIZATION_ID && hasEditorData) {
            LocalizationId = archive.ReadString();
        }

        if (FileVersionUE4 >= EObjectVersion.SERIALIZE_TEXT_IN_PACKAGES) {
            GatherableNameCount = archive.Read<int>();
            GatherableNameOffset = archive.Read<int>();
        }

        ExportCount = archive.Read<int>();
        ExportOffset = archive.Read<int>();
        ImportCount = archive.Read<int>();
        ImportOffset = archive.Read<int>();
        DependsOffset = archive.Read<int>();

        if (FileVersionUE4 >= EObjectVersion.ADD_STRING_ASSET_REFERENCES_MAP) {
            SoftPackageReferencesCount = archive.Read<int>();
            SoftPackageReferencesOffset = archive.Read<int>();
        }

        if (FileVersionUE4 >= EObjectVersion.ADDED_SEARCHABLE_NAMES) {
            SearchableNamesOffset = archive.Read<int>();
        }

        ThumbnailTableOffset = archive.Read<int>();
        Guid = archive.Read<Guid>();

        if (hasEditorData) {
            if (FileVersionUE4 >= EObjectVersion.ADDED_PACKAGE_OWNER) {
                PersistentGuid = archive.Read<Guid>();

                if (FileVersionUE4 < EObjectVersion.NON_OUTER_PACKAGE_IMPORT) {
                    OwnerPersistentGuid = archive.Read<Guid>();
                }
            }
        }

        Generations = archive.ReadArray<FGenerationInfo>().ToArray();

        SavedByEngineVersion = FileVersionUE4 >= EObjectVersion.ENGINE_VERSION_OBJECT ? new FEngineVersion(archive) : new FEngineVersion { Major = 4, Changeset = archive.Read<uint>() };

        if (FileVersionUE4 >= EObjectVersion.PACKAGE_SUMMARY_HAS_COMPATIBLE_ENGINE_VERSION) {
            CompatibleWithEngineVersion = new FEngineVersion(archive);
        }

        CompressionFlags = archive.Read<int>();

        CompressedChunks = archive.ReadArray<FCompressedChunk>().ToArray();

        PackageSource = archive.Read<uint>();
        AdditionalPackagesToCook = archive.ReadStrings();
        if (LegacyFileVersion > ELegacyFileVersion.TEXTURE_ALLOC_REMOVED) {
            NumTextureAllocations = archive.Read<int>();
        }

        AssetRegistryDataOffset = archive.Read<int>();
        BulkDataStartOffset = archive.Read<long>();
        if (FileVersionUE4 >= EObjectVersion.WORLD_LEVEL_INFO) {
            WorldTileInfoDataOffset = archive.Read<int>();
        }

        switch (FileVersionUE4) {
            case >= EObjectVersion.CHANGED_CHUNKID_TO_BE_AN_ARRAY_OF_CHUNKIDS:
                ChunkIDs = archive.ReadArray<int>().ToArray();
                break;
            case >= EObjectVersion.ADDED_CHUNKID_TO_ASSETDATA_AND_UPACKAGE: {
                var chunkId = archive.Read<int>();
                ChunkIDs = new[] { chunkId };
                break;
            }
            default:
                ChunkIDs = Array.Empty<int>();
                break;
        }

        if (FileVersionUE4 >= EObjectVersion.PRELOAD_DEPENDENCIES_IN_COOKED_EXPORTS) {
            PreloadDependencyCount = archive.Read<int>();
            PreloadDependencyOffset = archive.Read<int>();
        }

        if (FileVersionUE5 >= EObjectVersionUE5.NAMES_REFERENCED_FROM_EXPORT_DATA) {
            NamesReferencedFromExportDataCount = archive.Read<int>();
        }

        if(FileVersionUE5 >= EObjectVersionUE5.PAYLOAD_TOC) {
            PayloadTocOffset = archive.Read<int>();
        }
    }

    public uint Tag { get; }
    public ELegacyFileVersion LegacyFileVersion { get; }
    public int LegacyUE3Version { get; }
    public EObjectVersion FileVersionUE4 { get; }
    public EObjectVersionUE5 FileVersionUE5 { get; }
    public int FileVersionLicenseeUE4 { get; }
    public FCustomVersion[] CustomVersions { get; } = Array.Empty<FCustomVersion>();
    public int TotalHeaderSize { get; }
    public string? FolderName { get; }
    public string? LocalizationId { get; }
    public EPackageFlags PackageFlags { get; }
    public int NameCount { get; }
    public int NameOffset { get; }
    public int GatherableNameCount { get; }
    public int GatherableNameOffset { get; }
    public int ExportCount { get; }
    public int ExportOffset { get; }
    public int ImportCount { get; }
    public int ImportOffset { get; }
    public int DependsOffset { get; }
    public int SoftPackageReferencesCount { get; }
    public int SoftPackageReferencesOffset { get; }
    public int SearchableNamesOffset { get; }
    public int ThumbnailTableOffset { get; }
    public Guid Guid { get; }
    public Guid PersistentGuid { get; }
    public Guid OwnerPersistentGuid { get; }
    public FGenerationInfo[] Generations { get; } = Array.Empty<FGenerationInfo>();
    public FEngineVersion SavedByEngineVersion { get; } = new();
    public FEngineVersion CompatibleWithEngineVersion { get; } = new();
    public int CompressionFlags { get; }
    public FCompressedChunk[] CompressedChunks { get; } = Array.Empty<FCompressedChunk>();
    public uint PackageSource { get; }
    public string?[] AdditionalPackagesToCook { get; } = Array.Empty<string>();
    public int NumTextureAllocations { get; }
    public int AssetRegistryDataOffset { get; }
    public long BulkDataStartOffset { get; }
    public int WorldTileInfoDataOffset { get; }
    public int[] ChunkIDs { get; } = Array.Empty<int>();
    public int PreloadDependencyCount { get; }
    public int PreloadDependencyOffset { get; }
    public int NamesReferencedFromExportDataCount { get; }
    public int PayloadTocOffset { get; }
}
