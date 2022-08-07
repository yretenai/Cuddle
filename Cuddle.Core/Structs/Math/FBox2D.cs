using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("Box2f", "Box2D")]
public record FBox2D : FFallbackStruct {
    public FBox2D(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "Box2D") { }

    public FBox2D(FArchiveReader reader, string name) {
        var isDouble = name == "Box2D" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES;
        var vectorName = isDouble ? "Vector2D" : "Vector2f";
        Origin = new FVector2D(reader, vectorName);
        BoxExtent = new FVector2D(reader, vectorName);
        IsValid = reader.ReadBit();
    }

    public FVector2D Origin { get; set; }
    public FVector2D BoxExtent { get; set; }
    public bool IsValid { get; set; }
    public Box2D<double> ToSilk() => new(Origin.ToSilk(), BoxExtent.ToSilk());
}
