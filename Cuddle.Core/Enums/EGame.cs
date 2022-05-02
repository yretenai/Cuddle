﻿namespace Cuddle.Core.Enums;

// largely based on https://github.com/gildor2/UEViewer/blob/60accbff70e58bfc66eaad4594416694b95422ee/Unreal/UnCore.h#L344

// 32-bits 
// 00000000 00000000 00000000 00000000
// ue_major ue_minor ue_game  ue_game
// very unlikely that versions will ever go over 255, but more likely that games go over 255 given enough time.
public enum EGame {
    UE4_0 = 4 << 24, // 0b00000100 00000000 00000000 00000000
    UE4_1 = UE4_0 + (1 << 16),
    UE4_2 = UE4_0 + (2 << 16),
    UE4_3 = UE4_0 + (3 << 16),
    UE4_4 = UE4_0 + (4 << 16),
    UE4_5 = UE4_0 + (5 << 16),
    UE4_6 = UE4_0 + (6 << 16),
    UE4_7 = UE4_0 + (7 << 16),
    UE4_8 = UE4_0 + (8 << 16),
    UE4_9 = UE4_0 + (9 << 16),
    UE4_10 = UE4_0 + (10 << 16),
    UE4_11 = UE4_0 + (11 << 16),
    UE4_12 = UE4_0 + (12 << 16),
    UE4_13 = UE4_0 + (13 << 16),
    UE4_14 = UE4_0 + (14 << 16),
    UE4_15 = UE4_0 + (15 << 16),
    UE4_16 = UE4_0 + (16 << 16),
    UE4_17 = UE4_0 + (17 << 16),
    UE4_18 = UE4_0 + (18 << 16),
    UE4_19 = UE4_0 + (19 << 16),
    UE4_20 = UE4_0 + (20 << 16),
    UE4_21 = UE4_0 + (21 << 16),
    UE4_22 = UE4_0 + (22 << 16),
    UE4_23 = UE4_0 + (23 << 16),
    UE4_24 = UE4_0 + (24 << 16),
    UE4_25 = UE4_0 + (25 << 16),
    UE4_25Plus = UE4_25 + (1 << 14), // 0b00000100 00011001 01000000 00000000 -- leave 2 bits for 3 backport versions
    UE4_26 = UE4_0 + (26 << 16),
    UE4_27 = UE4_0 + (27 << 16),
    UE4_28 = UE4_0 + (28 << 16),

    UE5_0 = 5 << 24, // 0b00000101 00000000 00000000 00000000
    UE5_1 = UE4_0 + (1 << 16),

    UE4_MAX = UE4_28,
    UE5_MAX = UE5_1,
}