namespace Cuddle.Core.Structs.FileSystem;

public record struct FPakCompressedBlock {
    public long CompressedStart { get; set; }
    public long CompressedEnd { get; set; }
}
