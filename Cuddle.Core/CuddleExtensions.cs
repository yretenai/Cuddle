using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.Structs.Intl;

namespace Cuddle.Core;

public static class CuddleExtensions {
    public static EGame GetEngineVersion(this EGame game) => (EGame) ((uint) game & 0xFFFF0000);
    public static bool IsCustom(this EGame game) => ((uint) game & 0xFFFF) != 0;
    public static bool IsGame(this EGame game) => ((uint) game & 0x7FFF) != 0;
    public static bool IsBranch(this EGame game) => ((uint) game & 0x8000) != 0;

    public static string AsFormattedString(this EGame game) {
        var sb = new StringBuilder();
        var value = (uint) game;
        sb.Append(value >> 24);
        sb.Append('.');
        var minor = (value >> 16) & 0xFF;
        sb.Append(minor);
        if (!game.IsCustom()) {
            return sb.ToString();
        }

        sb.Append('.');

        var text = Enum.GetNames<EGame>().Zip(Enum.GetValues<EGame>()).Where(x => x.Second == game).Select(x => x.First.ToString()).MaxBy(x => x.Length);
        if (game.IsBranch() && !string.IsNullOrEmpty(text)) {
            var index = text.IndexOf('_');
            if (index > -1) {
                sb.Append(text[(index + 1 + (minor > 9 ? 2 : 1))..]);
                return sb.ToString();
            }
        }

        if (string.IsNullOrEmpty(text) || !text.StartsWith("GAME_")) {
            sb.Append(value & 0x7FFF);
            return sb.ToString();
        }

        sb.Append(text[5..].Replace('_', '-'));
        return sb.ToString();
    }

    public static EObjectVersion FindObjectVersion(this EGame game) {
        var version = game.ToGameObjectVersion();
        if (version == 0) {
            version = game.GetEngineVersion().ToObjectVersion();
        }

        return version;
    }

    public static EObjectVersionUE5 FindObjectVersionUE5(this EGame game) {
        var version = game.ToGameObjectVersionUE5();
        if (version == 0) {
            version = game.GetEngineVersion().ToObjectVersionUE5();
        }

        return version;
    }

    public static EObjectVersion ToGameObjectVersion(this EGame game) =>
        game switch {
            EGame.UE4_25Plus => EObjectVersion.ADDED_PACKAGE_OWNER,
            EGame.UE4_27Plus => EObjectVersion.CORRECT_LICENSEE_FLAG,
            _ => 0,
        };

    public static EObjectVersionUE5 ToGameObjectVersionUE5(this EGame game) =>
        game switch {
            _ => 0,
        };

    public static EObjectVersion ToObjectVersion(this EGame game) =>
        game switch {
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
            EGame.UE5_0 => EObjectVersion.CORRECT_LICENSEE_FLAG,
            EGame.UE5_1 => EObjectVersion.CORRECT_LICENSEE_FLAG,
            _ => EObjectVersion.NEWEST_LOADABLE_PACKAGE,
        };

    public static EObjectVersionUE5 ToObjectVersionUE5(this EGame game) =>
        game switch {
            EGame.UE5_0 => EObjectVersionUE5.LARGE_WORLD_COORDINATES,
            EGame.UE5_1 => EObjectVersionUE5.TRACK_OBJECT_EXPORT_IS_INHERITED,
            _ => EObjectVersionUE5.NEWEST_LOADABLE_PACKAGE,
        };

    public static EEditorObjectVersion FindEditorVersion(this EGame game) {
        var version = game.ToGameEditorVersion();
        if (version == 0) {
            version = game.GetEngineVersion().ToEditorVersion();
        }

        return version;
    }

    public static EEditorObjectVersion ToGameEditorVersion(this EGame game) =>
        game switch {
            EGame.UE4_25Plus => EEditorObjectVersion.SkeletalMeshBuildRefactor,
            EGame.UE4_27Plus => EEditorObjectVersion.SkeletalMeshSourceDataSupport16bitOfMaterialNumber,
            _ => 0,
        };

    public static EEditorObjectVersion ToEditorVersion(this EGame game) =>
        game switch {
            EGame.UE4_0 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_1 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_2 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_3 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_4 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_5 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_6 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_7 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_8 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_9 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_10 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_11 => EEditorObjectVersion.BeforeCustomVersionWasAdded,
            EGame.UE4_12 => EEditorObjectVersion.GatheredTextPackageCacheFixesV1,
            EGame.UE4_13 => EEditorObjectVersion.SplineComponentCurvesInStruct,
            EGame.UE4_14 => EEditorObjectVersion.RefactorMeshEditorMaterials,
            EGame.UE4_15 => EEditorObjectVersion.AddedInlineFontFaceAssets,
            EGame.UE4_16 => EEditorObjectVersion.MaterialThumbnailRenderingChanges,
            EGame.UE4_17 => EEditorObjectVersion.GatheredTextEditorOnlyPackageLocId,
            EGame.UE4_18 => EEditorObjectVersion.GatheredTextEditorOnlyPackageLocId,
            EGame.UE4_19 => EEditorObjectVersion.AddedMorphTargetSectionIndices,
            EGame.UE4_20 => EEditorObjectVersion.SerializeInstancedStaticMeshRenderData,
            EGame.UE4_21 => EEditorObjectVersion.MeshDescriptionNewAttributeFormat,
            EGame.UE4_22 => EEditorObjectVersion.MeshDescriptionRemovedHoles,
            EGame.UE4_23 => EEditorObjectVersion.RemoveLandscapeHoleMaterial,
            EGame.UE4_24 => EEditorObjectVersion.SkeletalMeshBuildRefactor,
            EGame.UE4_25 => EEditorObjectVersion.SkeletalMeshMoveEditorSourceDataToPrivateAsset,
            EGame.UE4_26 => EEditorObjectVersion.SkeletalMeshSourceDataSupport16bitOfMaterialNumber,
            EGame.UE4_27 => EEditorObjectVersion.SkeletalMeshSourceDataSupport16bitOfMaterialNumber,
            EGame.UE5_0 => EEditorObjectVersion.SkeletalMeshSourceDataSupport16bitOfMaterialNumber,
            EGame.UE5_1 => EEditorObjectVersion.SkeletalMeshSourceDataSupport16bitOfMaterialNumber,
            _ => EEditorObjectVersion.LatestVersion,
        };

    public static bool IsNullOrNone([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value) || value == "None";

    public static string ToISO_639_1(this ECulture culture) =>
        culture switch {
            ECulture.None => "xx",
            ECulture.English => "en",
            ECulture.EnglishUS => "en-US",
            ECulture.EnglishGB => "en-GB",
            ECulture.EnglishHK => "en-HK",
            ECulture.French => "fr",
            ECulture.FrenchFR => "fr-FR",
            ECulture.FrenchCA => "fr-CA",
            ECulture.Italian => "it",
            ECulture.ItalianIT => "it-IT",
            ECulture.German => "de",
            ECulture.GermanDE => "de-DE",
            ECulture.Spanish => "es",
            ECulture.SpanishES => "es-ES",
            ECulture.SpanishMX => "es-MX",
            ECulture.SpanishLatAm => "es-419",
            ECulture.Danish => "da",
            ECulture.DanishDK => "da-DK",
            ECulture.Dutch => "nl",
            ECulture.DutchNL => "nl-NL",
            ECulture.Finnish => "fi",
            ECulture.FinnishFI => "fi-FI",
            ECulture.Swedish => "sv",
            ECulture.SwedishSE => "sv-SE",
            ECulture.Russian => "ru",
            ECulture.RussianRU => "ru-RU",
            ECulture.Polish => "pl",
            ECulture.PolishPL => "pl-PL",
            ECulture.Arabic => "ar",
            ECulture.ArabicAE => "ar-AE",
            ECulture.Korean => "ko",
            ECulture.KoreanKR => "ko-KR",
            ECulture.Japanese => "ja",
            ECulture.JapaneseJP => "ja-JP",
            ECulture.SimplifiedChinese => "zh-Hans",
            ECulture.SimplifiedChineseCN => "zh-Hans-CN",
            ECulture.SimplifiedChineseHK => "zh-Hans-HK",
            ECulture.SimplifiedChineseMO => "zh-Hans-MO",
            ECulture.SimplifiedChineseSG => "zh-Hans-SG",
            ECulture.TraditionalChinese => "zh-Hant",
            ECulture.TraditionalChineseCN => "zh-Hant-CN",
            ECulture.TraditionalChineseHK => "zh-Hant-HK",
            ECulture.TraditionalChineseMO => "zh-Hant-MO",
            ECulture.TraditionalChineseSG => "zh-Hant-SG",
            ECulture.Turkish => "tr",
            ECulture.TurkishTR => "tr-TR",
            ECulture.Thai => "th",
            ECulture.ThaiTH => "th-TH",
            ECulture.Portuguese => "pt",
            ECulture.PortuguesePT => "pt-PT",
            ECulture.PortugueseBR => "pt-BR",
            _ => throw new ArgumentOutOfRangeException(nameof(culture), culture, null),
        };
}
