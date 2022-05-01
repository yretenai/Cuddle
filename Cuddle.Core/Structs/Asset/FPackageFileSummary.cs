using System;
using System.Linq;
using Cuddle.Core.Enums;
using Cuddle.Core.FileSystem;
using DragonLib.IO;

namespace Cuddle.Core.Structs.Asset;

// UE4 reference: FPackageFileSummary::Serialize (PackageFileSummary.h)
// https://github.com/gildor2/UEViewer/blob/60accbff70e58bfc66eaad4594416694b95422ee/Unreal/UnrealPackage/UnPackage4.cpp
public class FPackageFileSummary {
    public FPackageFileSummary() { }

    public FPackageFileSummary(FArchive archive) {
        Tag = archive.Read<uint>();
        Logger.Assert(Tag == 0x9E2A83C1, "Tag == 0x9E2A83C1", "Tag does not match expected asset magic tag", $"Got {Tag:X8} instead!");
        LegacyFileVersion = archive.Read<ELegacyFileVersion>();

        LegacyUE3Version = LegacyFileVersion switch {
            > ELegacyFileVersion.OLDEST_LOADABLE_PACKAGE => throw new NotSupportedException(),
            < ELegacyFileVersion.NEWEST_LOADABLE_PACKAGE => throw new NotSupportedException(),
            ELegacyFileVersion.REMOVED_UE3_VERSION => archive.Read<int>(),
            _ => LegacyUE3Version,
        };

        FileVersionUE4 = archive.Read<EObjectVersion>();
        var isUnversioned = FileVersionUE4 == 0;
        if (FileVersionUE4 == 0) {
            // https://github.com/gildor2/UEViewer/blob/60accbff70e58bfc66eaad4594416694b95422ee/Unreal/UE4Version.h#L7
            FileVersionUE4 = archive.Game switch {
                EGame.UE4_25Plus => EObjectVersion.ADDED_PACKAGE_OWNER,
                _ => 0,
            };

            if (FileVersionUE4 == 0) {
                FileVersionUE4 = archive.Game.GetEngineVersion() switch {
                    EGame.UE4_0 => EObjectVersion.PRIVATE_REMOTE_ROLE,
                    EGame.UE4_1 => EObjectVersion.UNDO_BREAK_MATERIALATTRIBUTES_CHANGE,
                    EGame.UE4_2 => EObjectVersion.FIX_MATERIAL_COORDS,
                    EGame.UE4_3 => EObjectVersion.FIX_MATERIAL_PROPERTY_OVERRIDE_SERIALIZE,
                    EGame.UE4_4 => EObjectVersion.BLUEPRINT_USE_SCS_ROOTCOMPONENT_SCALE,
                    EGame.UE4_5 => EObjectVersion.RENAME_CAMERA_COMPONENT_CONTROL_ROTATION,
                    EGame.UE4_6 => EObjectVersion.MOVEMENTCOMPONENT_AXIS_SETTINGS,
                    EGame.UE4_7 => EObjectVersion.AFTER_MERGING_ADD_MODIFIERS_RUNTIME_GENERATION_TO_4_7,
                    EGame.UE4_8 => EObjectVersion.SERIALIZE_BLUEPRINT_EVENTGRAPH_FASTCALLS_IN_UFUNCTION,
                    EGame.UE4_9 => EObjectVersion.APEX_CLOTH_TESSELLATION,
                    EGame.UE4_10 => EObjectVersion.APEX_CLOTH_TESSELLATION,
                    EGame.UE4_11 => EObjectVersion.STREAMABLE_TEXTURE_MIN_MAX_DISTANCE,
                    EGame.UE4_12 => EObjectVersion.NAME_HASHES_SERIALIZED,
                    EGame.UE4_13 => EObjectVersion.INSTANCED_STEREO_UNIFORM_REFACTOR,
                    EGame.UE4_14 => EObjectVersion.TemplateIndex_IN_COOKED_EXPORTS,
                    EGame.UE4_15 => EObjectVersion.ADDED_SEARCHABLE_NAMES,
                    EGame.UE4_16 => EObjectVersion.ADDED_SWEEP_WHILE_WALKING_FLAG,
                    EGame.UE4_17 => EObjectVersion.ADDED_SWEEP_WHILE_WALKING_FLAG,
                    EGame.UE4_18 => EObjectVersion.ADDED_SOFT_OBJECT_PATH,
                    EGame.UE4_19 => EObjectVersion.ADDED_PACKAGE_SUMMARY_LOCALIZATION_ID,
                    EGame.UE4_20 => EObjectVersion.ADDED_PACKAGE_SUMMARY_LOCALIZATION_ID,
                    EGame.UE4_21 => EObjectVersion.FIX_WIDE_STRING_CRC,
                    EGame.UE4_22 => EObjectVersion.FIX_WIDE_STRING_CRC,
                    EGame.UE4_23 => EObjectVersion.FIX_WIDE_STRING_CRC,
                    EGame.UE4_24 => EObjectVersion.ADDED_PACKAGE_OWNER,
                    EGame.UE4_25 => EObjectVersion.ADDED_PACKAGE_OWNER,
                    EGame.UE4_26 => EObjectVersion.CORRECT_LICENSEE_FLAG,
                    EGame.UE4_27 => EObjectVersion.CORRECT_LICENSEE_FLAG,
                    _ => 0,
                };
            }
        }

        if (FileVersionUE4 is < EObjectVersion.NEWEST_LOADABLE_PACKAGE or < EObjectVersion.OLDEST_LOADABLE_PACKAGE) {
            throw new NotSupportedException();
        }

        if (LegacyFileVersion <= ELegacyFileVersion.ADDED_UE5_VERSION) {
            FileVersionUE5 = archive.Read<int>();
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

        SavedByEngineVersion = FileVersionUE4 >= EObjectVersion.ENGINE_VERSION_OBJECT ? archive.Read<FEngineVersion>() : new FEngineVersion { Major = 4, Changeset = archive.Read<uint>() };

        if (FileVersionUE4 >= EObjectVersion.PACKAGE_SUMMARY_HAS_COMPATIBLE_ENGINE_VERSION) {
            CompatibleWithEngineVersion = archive.Read<FEngineVersion>();
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
    }

    public uint Tag { get; }
    public ELegacyFileVersion LegacyFileVersion { get; }
    public int LegacyUE3Version { get; }
    public EObjectVersion FileVersionUE4 { get; }
    public int FileVersionUE5 { get; }
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
    public FEngineVersion SavedByEngineVersion { get; }
    public FEngineVersion CompatibleWithEngineVersion { get; }
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
}
