using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Asset;

public class FPackageTrailerHeader {
    public FPackageTrailerHeader(FArchiveReader archive) {
        Tag = archive.Read<ulong>();
        Version = archive.Read<EPackageTrailerVersion>();
        HeaderLength = archive.Read<int>();
        PayloadsDataLength = archive.Read<int>();
        if (Version < EPackageTrailerVersion.ACCESS_PER_PAYLOAD) {
            AccessMode = archive.Read<EPayloadAccessMode>();
        }
    }

    public ulong Tag { get; set; }
    public EPackageTrailerVersion Version { get; set; }
    public int HeaderLength { get; set; }
    public int PayloadsDataLength { get; set; }
    public EPayloadAccessMode AccessMode { get; set; }
}
