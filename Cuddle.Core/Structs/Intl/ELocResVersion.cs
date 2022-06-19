namespace Cuddle.Core.Structs.Intl;

public enum ELocResVersion : byte {
    Legacy = 0,
    Compact = 1,
    CRC32 = 2,
    CityHash64 = 3,
}
