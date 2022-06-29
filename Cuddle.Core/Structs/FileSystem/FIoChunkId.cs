using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Cuddle.Core.Structs.FileSystem;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
public readonly record struct FIoChunkId(ulong Id, ushort ChunkIdBE, byte ChunkType) {
    public ushort ChunkId => BinaryPrimitives.ReverseEndianness(ChunkIdBE);
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 10)]
public readonly record struct FIoOffsetAndLength(Int40BE Offset, Int40BE Length);

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
public readonly record struct FIoStoreTocCompressedBlockEntry(Int40 Offset, Int20 Size, Int20 UncompressedSize, byte CompressionMethod);
