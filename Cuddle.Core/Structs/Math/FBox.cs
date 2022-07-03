using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("FBox3f", "FBox3d", "FBox")]
public record FBox : FFallbackStruct {
    public FBox(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "Box") { }

    public FBox(FArchiveReader reader, string name) {
        var isDouble = name == "Box" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES || name == "Box3d";
        var vectorName = isDouble ? "Vector3d" : "Vector3f";
        Origin = new FVector(reader, vectorName);
        BoxExtent = new FVector(reader, vectorName);
        IsValid = reader.ReadBoolean();
    }

    public FVector Origin { get; set; }
    public FVector BoxExtent { get; set; }
    public bool IsValid { get; set; }
    public Box3D<double> ToSilk() => new(Origin.ToSilk(), BoxExtent.ToSilk());
}
