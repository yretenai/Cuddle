using System;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core;

public interface IVFSEntry : IDisposable {
    UPakFile Owner { get; }
    long Size { get; }
    string MountedPath { get; }
    ulong MountedPathHash { get; }
    object? Data { get; set; }
    bool Disposed { get; }
    MemoryOwner<byte> ReadFile();
    UAssetFile? ReadAsset();
    UObject? ReadAssetExport(int index);
    UObject?[] ReadAssetExports();
}
