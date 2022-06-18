namespace Cuddle.Core.Structs.Asset;

public record struct FCompressedChunk {
    public int UncompressedOffset { get; }
    public int UncompressedSize { get; }
    public int CompressedOffset { get; }
    public int CompressedSize { get; }
}
