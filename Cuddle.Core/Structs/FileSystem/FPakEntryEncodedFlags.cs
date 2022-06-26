namespace Cuddle.Core.Structs.FileSystem;

// UE4 reference: FPakFile::DecodePakEntry
public readonly record struct FPakEntryEncodedFlags {
    public FPakEntryEncodedFlags(uint value) {
        CompressionBlockSize = (int) (value & 0x3f);
        CompressionBlockCount = (int) ((value >> 6) & 0xffff);
        Encrypted = (value & 0x400000) == 0x400000;
        CompressionMethod = (int) ((value >> 23) & 0x3f);
        SizeIs32BitSafe = (value & 0x20000000) == 0x20000000;
        UncompressedSizeIs32BitSafe = (value & 0x40000000) == 0x40000000;
        OffsetSizeIs32BitSafe = (value & 0x80000000) == 0x80000000;
    }

    public int CompressionBlockSize { get; }
    public int CompressionBlockCount { get; }
    public bool Encrypted { get; }
    public int CompressionMethod { get; }
    public bool SizeIs32BitSafe { get; }
    public bool UncompressedSizeIs32BitSafe { get; }

    public bool OffsetSizeIs32BitSafe { get; }
}
