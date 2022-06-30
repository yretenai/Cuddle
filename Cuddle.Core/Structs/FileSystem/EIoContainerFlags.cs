using System;

namespace Cuddle.Core.Structs.FileSystem;

[Flags]
public enum EIoContainerFlags : byte {
    Compressed = 1,
    Encrypted = 2,
    Signed = 4,
    Indexed = 8,
}
