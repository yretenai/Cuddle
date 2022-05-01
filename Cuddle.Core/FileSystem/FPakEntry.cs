using System;

namespace Cuddle.Core.FileSystem;

// umodel reference: https://github.com/gildor2/UEViewer/blob/c444911a6ad65bff5266f273dd5bdf7dd6fb506e/Unreal/FileSystem/UnArchivePak.h#L69
public class FPakEntry {
    public FPakEntry() { }

    public FPakEntry(FArchive archive) {
        // todo
    }

    public long Pos { get; set; }
    public long Size { get; set; }
    public long UncompressedSize { get; set; }
    public int CompressionMethod { get; set; }
    public int CompressionBlockSize { get; set; }
    public FPakCompressedBlock[] CompressionBlocks { get; set; } = Array.Empty<FPakCompressedBlock>();
    public FPakEntryFlags Flags { get; set; }
}
