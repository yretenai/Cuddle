using System;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Asset;

public class FPackageTrailer {
    public FPackageTrailer(FArchiveReader archive) {
        Header = new FPackageTrailerHeader(archive);
        LookupTable = archive.ReadClassArray<FPackageLookupTableEntry>(null, Header);
        Payload = archive.ReadArray<byte>(Header.PayloadsDataLength).ToArray();
        Footer = new FPackageTrailerFooter(archive);
    }

    public FPackageTrailerHeader Header { get; set; }
    public FPackageLookupTableEntry[] LookupTable { get; set; }
    public Memory<byte> Payload { get; set; }
    public FPackageTrailerFooter Footer { get; set; }
}
