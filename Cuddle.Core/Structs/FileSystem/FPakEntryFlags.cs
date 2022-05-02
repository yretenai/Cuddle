using DragonLib.IO;

namespace Cuddle.Core.Structs.FileSystem;

// UE4 reference: FPakFile::DecodePakEntry
public struct FPakEntryFlags {
    [BitField(6)]
    public int CompressionBlockSize { get; set; }

    [BitField(16)]
    public int CompressionBlockCount { get; set; }

    [BitField(1)]
    public bool Encrypted { get; set; }

    [BitField(6)]
    public int CompressionMethod { get; set; }

    [BitField(1)]
    public bool SizeIs32BitSafe { get; set; }

    [BitField(1)]
    public bool UncompressedSizeIs32BitSafe { get; set; }

    [BitField(1)]
    public bool OffsetSizeIs32BitSafe { get; set; }
}
