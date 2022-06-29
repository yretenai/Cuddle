using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cuddle.Core.Structs.FileSystem;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core.VFS;

public class FIoToc {
    public FIoToc(FIoStore store, FileStream tocStream) {
        Owner = store;

        Span<byte> buffer = stackalloc byte[16];
        if (tocStream.Read(buffer) != 16) {
            throw new InvalidDataException("Toc stream is too short");
        }

        if (BinaryPrimitives.ReadUInt64LittleEndian(buffer) != 0x2D3D3D2D2D3D3D2D &&
            BinaryPrimitives.ReadUInt64LittleEndian(buffer[8..]) != 0x2D3D3D2D2D3D3D2D) {
            throw new InvalidDataException("Toc stream is not a valid TOC");
        }

        buffer = stackalloc byte[8];
        if (tocStream.Read(buffer) != 8) {
            throw new InvalidDataException("Toc stream is too short");
        }

        Version = (EIoStoreTocVersion)BinaryPrimitives.ReadInt32LittleEndian(buffer);
        var TocHeaderSize = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);

        buffer = stackalloc byte[TocHeaderSize - 24];
        if (tocStream.Read(buffer) != buffer.Length) {
            throw new InvalidDataException("Toc stream is too short");
        }

        var tocEntryCount = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        var tocCompressedBlockEntryCount = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);
        var _ = BinaryPrimitives.ReadInt32LittleEndian(buffer[8..]); // tocCompressedBlockEntrySize
        var compressionMethodNameCount = BinaryPrimitives.ReadInt32LittleEndian(buffer[12..]);
        var compressionMethodNameLength = BinaryPrimitives.ReadInt32LittleEndian(buffer[16..]);
        CompressionBlockSize = BinaryPrimitives.ReadInt32LittleEndian(buffer[20..]);
        var directoryIndexSize = BinaryPrimitives.ReadInt32LittleEndian(buffer[24..]);
        PartitionCount = BinaryPrimitives.ReadInt32LittleEndian(buffer[28..]);
        ContainerId = BinaryPrimitives.ReadInt64LittleEndian(buffer[32..]);
        EncryptionGuid = MemoryMarshal.Read<Guid>(buffer[36..]);
        ContainerFlags = BinaryPrimitives.ReadInt64LittleEndian(buffer[52..]);
        PartitionSize = BinaryPrimitives.ReadInt64LittleEndian(buffer[56..]);

        if (Version < EIoStoreTocVersion.PartitionSize) {
            PartitionCount = 1;
            PartitionSize = long.MaxValue;
        }

        ChunkIds = new FIoChunkId[tocEntryCount].AsMemory();
        ChunkOffsetLengths = new FIoOffsetAndLength[tocEntryCount].AsMemory();
        CompressionBlocks = new FIoStoreTocCompressedBlockEntry[tocCompressedBlockEntryCount].AsMemory();

        if (tocStream.Read(MemoryMarshal.AsBytes(ChunkIds.Span)) != tocEntryCount * Unsafe.SizeOf<FIoChunkId>()) {
            throw new InvalidDataException("Toc stream is too short");
        }

        if (tocStream.Read(MemoryMarshal.AsBytes(ChunkOffsetLengths.Span)) != tocEntryCount * Unsafe.SizeOf<FIoOffsetAndLength>()) {
            throw new InvalidDataException("Toc stream is too short");
        }

        if (tocStream.Read(MemoryMarshal.AsBytes(CompressionBlocks.Span)) != tocCompressedBlockEntryCount * Unsafe.SizeOf<FIoStoreTocCompressedBlockEntry>()) {
            throw new InvalidDataException("Toc stream is too short");
        }

        CompressionMethods = new List<string> {
            "None",
        };

        using var pooled = MemoryOwner<byte>.Allocate(compressionMethodNameLength);
        if (tocStream.Read(pooled.Span) != compressionMethodNameLength) {
            throw new InvalidDataException("Toc stream is too short");
        }

        using var reader = new FArchiveReader(pooled);
        for (var i = 0; i < compressionMethodNameCount; ++i) {
            CompressionMethods.Add(reader.ReadString());
        }
    }

    public FIoStore Owner { get; }
    public EIoStoreTocVersion Version { get; set; }
    public int CompressionBlockSize { get; set; }
    public List<string> CompressionMethods { get; set; }
    public long ContainerFlags { get; set; }
    public long ContainerId { get; set; }
    public Guid EncryptionGuid { get; set; }
    public int PartitionCount { get; set; }
    public long PartitionSize { get; set; }
    public Memory<FIoChunkId> ChunkIds { get; set; }
    public Memory<FIoOffsetAndLength> ChunkOffsetLengths { get; set; }
    public Memory<FIoStoreTocCompressedBlockEntry> CompressionBlocks { get; set; }
}
