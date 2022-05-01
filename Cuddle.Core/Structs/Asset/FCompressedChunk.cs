namespace Cuddle.Core.Structs.Asset;

public struct FCompressedChunk {
    public int UncompressedOffset { get; set; }
    public int UncompressedSize { get; set; }
    public int CompressedOffset { get; set; }
    public int CompressedSize { get; set; }
}
