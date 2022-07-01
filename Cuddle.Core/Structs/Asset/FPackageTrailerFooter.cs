using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Asset;

public class FPackageTrailerFooter {
    public FPackageTrailerFooter(FArchiveReader archive) {
        Tag = archive.Read<ulong>();
        TrailerLength = archive.Read<int>();
        PackageTag = archive.Read<uint>();
    }

    public ulong Tag { get; set; }
    public int TrailerLength { get; set; }
    public uint PackageTag { get; set; }
}
