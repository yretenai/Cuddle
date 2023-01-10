using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Cuddle.Core.VFS;
using Cuddle.Security;
using DragonLib;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core.Structs.FileSystem;

public sealed class FIoToc : IPoliteDisposable {
    public FIoToc(FIoStore store, Stream stream, AESKeyStore keyStore) {
        Owner = store;

        Span<byte> buffer = stackalloc byte[16];
        stream.ReadExactly(buffer);

        if (BinaryPrimitives.ReadUInt64LittleEndian(buffer) != 0x2D3D3D2D2D3D3D2D &&
            BinaryPrimitives.ReadUInt64LittleEndian(buffer[8..]) != 0x2D3D3D2D2D3D3D2D) {
            throw new InvalidDataException("Toc stream is not valid");
        }

        buffer = stackalloc byte[8];
        stream.ReadExactly(buffer);

        Version = (EIoStoreTocVersion) BinaryPrimitives.ReadInt32LittleEndian(buffer);
        var TocHeaderSize = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);

        buffer = stackalloc byte[TocHeaderSize - 24];
        stream.ReadExactly(buffer);

        var tocEntryCount = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        var tocCompressedBlockEntryCount = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);
        var _ = BinaryPrimitives.ReadInt32LittleEndian(buffer[8..]); // tocCompressedBlockEntrySize
        var compressionMethodNameCount = BinaryPrimitives.ReadInt32LittleEndian(buffer[12..]);
        var compressionMethodNameLength = BinaryPrimitives.ReadInt32LittleEndian(buffer[16..]);
        CompressionBlockSize = BinaryPrimitives.ReadInt32LittleEndian(buffer[20..]);
        var directoryIndexSize = BinaryPrimitives.ReadInt32LittleEndian(buffer[24..]);
        PartitionCount = BinaryPrimitives.ReadInt32LittleEndian(buffer[28..]);
        ContainerId = BinaryPrimitives.ReadInt64LittleEndian(buffer[32..]);
        EncryptionGuid = MemoryMarshal.Read<Guid>(buffer[40..]);
        ContainerFlags = (EIoContainerFlags) buffer[56];
        // 3 bytes of padding.
        var tocChunkPerfectHashSeedsCount = BinaryPrimitives.ReadInt32LittleEndian(buffer[60..]);
        PartitionSize = BinaryPrimitives.ReadInt64LittleEndian(buffer[64..]);
        var tocChunksWithoutPerfectHashCount = BinaryPrimitives.ReadInt32LittleEndian(buffer[72..]);

        if (Version < EIoStoreTocVersion.PartitionSize) {
            PartitionCount = 1;
            PartitionSize = long.MaxValue;
        }

        if (Version < EIoStoreTocVersion.PerfectHash) {
            tocChunkPerfectHashSeedsCount = 0;
        }

        if (Version < EIoStoreTocVersion.PerfectHashWithOverflow) {
            tocChunksWithoutPerfectHashCount = 0;
        }

        ChunkIds = new FIoChunkId[tocEntryCount].AsMemory();
        ChunkOffsetLengths = new FIoOffsetAndLength[tocEntryCount].AsMemory();
        HashSeeds = new int[tocChunkPerfectHashSeedsCount].AsMemory();
        ChunkIndicesWithoutHash = new int[tocChunksWithoutPerfectHashCount].AsMemory();
        CompressionBlocks = new FIoStoreTocCompressedBlockEntry[tocCompressedBlockEntryCount].AsMemory();
        stream.ReadExactly(MemoryMarshal.AsBytes(ChunkIds.Span));
        stream.ReadExactly(MemoryMarshal.AsBytes(ChunkOffsetLengths.Span));
        stream.ReadExactly(MemoryMarshal.AsBytes(HashSeeds.Span));
        stream.ReadExactly(MemoryMarshal.AsBytes(ChunkIndicesWithoutHash.Span));
        stream.ReadExactly(MemoryMarshal.AsBytes(CompressionBlocks.Span));

        CompressionMethods = new List<string> {
            "None",
        };

        using var pooled = MemoryOwner<byte>.Allocate(compressionMethodNameLength * compressionMethodNameCount);
        stream.ReadExactly(pooled.Span);

        for (var i = 0; i < compressionMethodNameCount; ++i) {
            CompressionMethods.Add(pooled.Span[(i * compressionMethodNameLength)..].ReadUTF8String() ?? throw new InvalidDataException());
        }

        if (ContainerFlags.HasFlag(EIoContainerFlags.Signed)) {
            buffer = stackalloc byte[4];
            stream.ReadExactly(buffer);
            var hashSize = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            TocSignature = new byte[hashSize].AsMemory();
            BlockSignature = new byte[hashSize].AsMemory();
            stream.ReadExactly(MemoryMarshal.AsBytes(TocSignature.Span));
            stream.ReadExactly(MemoryMarshal.AsBytes(BlockSignature.Span));
            var data = new byte[20 * tocCompressedBlockEntryCount];
            stream.ReadExactly(data);
            ChunkSignatures = new Memory2D<byte>(data, tocCompressedBlockEntryCount, 20);
        }

        if (Version >= EIoStoreTocVersion.DirectoryIndex && ContainerFlags.HasFlag(EIoContainerFlags.Indexed)) {
            DirectoryIndexBuffer = MemoryOwner<byte>.Allocate(directoryIndexSize);
            stream.ReadExactly(DirectoryIndexBuffer.Span);
            // replicate unreal behavior, since it can be encrypted and decryption is handled in FIoStore.
        }

        // todo: meta
    }

    public MemoryOwner<byte>? DirectoryIndexBuffer { get; set; }

    public FIoStore Owner { get; }
    public EIoStoreTocVersion Version { get; set; }
    public int CompressionBlockSize { get; set; }
    public List<string> CompressionMethods { get; set; }
    public EIoContainerFlags ContainerFlags { get; set; }
    public long ContainerId { get; set; }
    public Guid EncryptionGuid { get; set; }
    public int PartitionCount { get; set; }
    public long PartitionSize { get; set; }
    public Memory<FIoChunkId> ChunkIds { get; set; }
    public Memory<FIoOffsetAndLength> ChunkOffsetLengths { get; set; }
    public Memory<FIoStoreTocCompressedBlockEntry> CompressionBlocks { get; set; }
    public Memory<int> HashSeeds { get; set; }
    public Memory<int> ChunkIndicesWithoutHash { get; set; }
    public Memory<byte> TocSignature { get; set; } = Memory<byte>.Empty;
    public Memory<byte> BlockSignature { get; set; } = Memory<byte>.Empty;
    public Memory2D<byte> ChunkSignatures { get; set; } = Memory2D<byte>.Empty;

    public void Dispose() {
        if (Disposed) {
            return;
        }

        DirectoryIndexBuffer?.Dispose();
        Disposed = true;
    }

    public bool Disposed { get; private set; }
}
