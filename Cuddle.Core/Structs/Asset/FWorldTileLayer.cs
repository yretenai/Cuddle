using Cuddle.Core.Structs.Math;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Asset;

public record FWorldTileLayer {
    public FWorldTileLayer(FArchiveReader reader) {
        Name = reader.ReadString();
        Reserved0 = reader.Read<int>();
        Reserved1 = reader.Read<FIntPoint>();

        if (reader.Version >= EObjectVersion.WORLD_LEVEL_INFO_UPDATED) {
            StreamingDistance = reader.Read<int>();
        }

        if (reader.Version >= EObjectVersion.WORLD_LAYER_ENABLE_DISTANCE_STREAMING) {
            DistanceStreamingEnabled = reader.ReadBoolean();
        }
    }

    public string Name { get; set; }
    public int Reserved0 { get; set; }
    public FIntPoint Reserved1 { get; set; }
    public int StreamingDistance { get; set; }
    public bool DistanceStreamingEnabled { get; set; }
}
