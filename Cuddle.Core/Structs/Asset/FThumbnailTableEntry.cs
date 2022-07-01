using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Asset;

public record FThumbnailTableEntry {
    public FThumbnailTableEntry() {
        AssetClass = "None";
        ObjectPath = "None";
    }

    public FThumbnailTableEntry(FArchiveReader reader) {
        AssetClass = reader.ReadString();
        ObjectPath = reader.ReadString();
        Offset = reader.Read<int>();
    }

    public string AssetClass { get; set; }
    public string ObjectPath { get; set; }
    public int Offset { get; set; }
}
