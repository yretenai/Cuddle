using System;

namespace Cuddle.Core.Structs.FileSystem;

[Flags]
// UE4 reference: FPakFile -> Flag_None, Flag_Encrypted, Flag_Deleted.
public enum FPakEntryFlags : byte {
    None = 0,
    Encrypted = 1,
    Deleted = 2,
}
