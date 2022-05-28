using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core;

public interface IVFSEntry {
    UPakFile Owner { get; }
    long Size { get; }
    string MountedPath { get; }
    ulong MountedPathHash { get; }
    MemoryOwner<byte> ReadFile();
    UObject? ReadAssetExport(int index);
    UObject?[] ReadAssetExports();
}
