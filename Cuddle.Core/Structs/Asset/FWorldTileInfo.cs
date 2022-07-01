using System;
using Cuddle.Core.Structs.Math;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Asset;

public record FWorldTileInfo {
    public FWorldTileInfo(FArchiveReader reader) {
        Position = reader.Read<FIntVector>();
        Bounds = new FBox(reader, "FBox");
        Layer = new FWorldTileLayer(reader);

        if (reader.Version >= EObjectVersion.WORLD_LEVEL_INFO_UPDATED) {
            HideInTileView = reader.ReadBoolean();
            ParentTilePackage = reader.ReadString();
        } else {
            ParentTilePackage = "None";
        }

        LODList = reader.Version >= EObjectVersion.WORLD_LEVEL_INFO_LOD_LIST ? reader.ReadArray<FWorldTileLODInfo>().ToArray() : Array.Empty<FWorldTileLODInfo>();

        if (reader.Version >= EObjectVersion.WORLD_LEVEL_INFO_ZORDER) {
            ZOrder = reader.Read<int>();
        }

        AbsolutePosition = reader.Read<FIntVector>();
    }

    public FIntVector Position { get; set; }
    public FIntVector AbsolutePosition { get; set; }
    public FBox Bounds { get; set; }
    public FWorldTileLayer Layer { get; set; }
    public bool HideInTileView { get; set; }
    public string ParentTilePackage { get; set; }
    public FWorldTileLODInfo[] LODList { get; set; }
    public int ZOrder { get; set; }
}
