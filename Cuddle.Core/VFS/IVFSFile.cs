using System;
using System.Collections.Generic;
using Cuddle.Core.Structs;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core.VFS;

public interface IVFSFile : IPoliteDisposable {
    VFSManager Manager { get; }
    string Name { get; }
    EGame Game { get; }
    Guid EncryptionGuid { get; }
    byte[]? EncryptionKey { get; set; }
    bool HasHashes { get; }
    bool HasPaths { get; }
    IEnumerable<IVFSEntry> Entries { get; }
    MemoryOwner<byte> ReadFile(string path);
    MemoryOwner<byte> ReadFile(ulong hash);
    MemoryOwner<byte> ReadFile(IVFSEntry entry);
    MemoryOwner<byte> ReadBytes(long offset, long count, bool isEncrypted);
    void ClearCaches();
}
