using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Asset;

public class FPackageLookupTableEntry {
    public FPackageLookupTableEntry() => Hash = new byte[20];

    public FPackageLookupTableEntry(FArchiveReader reader, FPackageTrailerHeader header) {
        Hash = reader.ReadArray<byte>(20).ToArray();
        OffsetInFile = reader.Read<long>();
        CompressedSize = reader.Read<long>();
        Size = reader.Read<long>();

        Flags = header.Version >= EPackageTrailerVersion.PAYLOAD_FLAGS ? reader.Read<EPayloadFlags>() : EPayloadFlags.None;
        AccessMode = header.Version >= EPackageTrailerVersion.ACCESS_PER_PAYLOAD ? reader.Read<EPayloadAccessMode>() : header.AccessMode;
    }

    public byte[] Hash { get; set; }
    public long OffsetInFile { get; set; }
    public long CompressedSize { get; set; }
    public long Size { get; set; }
    public EPayloadFlags Flags { get; set; }
    public EPayloadAccessMode AccessMode { get; set; }
}
