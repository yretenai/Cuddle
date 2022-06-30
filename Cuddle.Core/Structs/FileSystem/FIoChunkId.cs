using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Cuddle.Core.Structs.FileSystem;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
public readonly record struct FIoChunkId(ulong Id, ushort IndexBE, byte Reserved, byte ChunkType) {
    public ushort Index => BinaryPrimitives.ReverseEndianness(IndexBE);
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 10)]
public readonly record struct FIoOffsetAndLength(Int40BE Offset, Int40BE Length);

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
public readonly record struct FIoStoreTocCompressedBlockEntry(Int40 Offset, Int24 Size, Int24 UncompressedSize, byte CompressionMethod);
