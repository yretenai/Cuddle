using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("Vector2f", "Vector2D")]
public record FVector2D : FFallbackStruct {
    public FVector2D() { }
    public FVector2D(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "Vector2D") { }

    public FVector2D(FArchiveReader reader, string name) {
        var isDouble = name == "Vector2D" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES;
        X = isDouble ? reader.Read<double>() : reader.Read<float>();
        Y = isDouble ? reader.Read<double>() : reader.Read<float>();
    }

    public double X { get; set; }
    public double Y { get; set; }
    public Vector2D<double> ToSilk() => new(X, Y);
}
