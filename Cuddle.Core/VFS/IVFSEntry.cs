using System;
using Cuddle.Core.Assets;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core.VFS;

public interface IVFSEntry : IDisposable {
    IVFSFile Owner { get; }
    long Size { get; }
    string MountedPath { get; }
    string ObjectPath { get; }
    ulong MountedHash { get; }
    object? Data { get; set; }
    bool Disposed { get; }
    MemoryOwner<byte> ReadFile();
    UAssetFile? ReadAsset();
    UObject? ReadAssetExport(int index);
    UObject?[] ReadAssetExports();
}
