using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("BoxSphereBounds3f", "BoxSphereBounds3d", "BoxSphereBounds")]
public record FBoxSphereBounds : FFallbackStruct {
    public FBoxSphereBounds(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "BoxSphereBounds") { }

    public FBoxSphereBounds(FArchiveReader reader, string name) {
        var isDouble = name == "BoxSphereBounds" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES || name == "BoxSphereBounds3d";
        var vectorName = isDouble ? "Vector3d" : "Vector3f";
        Origin = new FVector(reader, vectorName);
        BoxExtent = new FVector(reader, vectorName);
        SphereRadius = isDouble ? reader.Read<double>() : reader.Read<float>();
    }

    public FVector Origin { get; set; }
    public FVector BoxExtent { get; set; }
    public double SphereRadius { get; set; }
}
