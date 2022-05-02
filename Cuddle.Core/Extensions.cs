﻿using Cuddle.Core.Enums;

namespace Cuddle.Core;

public static class Extensions {
    public static EGame GetEngineVersion(this EGame game) => (EGame) ((uint) game & 0xFFFF0000);

    public static EObjectVersion ToGameObjectVersion(this EGame game) =>
        game switch {
            EGame.UE4_25Plus => EObjectVersion.ADDED_PACKAGE_OWNER,
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
            _ => 0,
        };
}