using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("Vector3f", "Vector3d", "Vector")]
public record FVector : FFallbackStruct {
    public FVector() { }
    public FVector(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "Vector") { }

    public FVector(FArchiveReader reader, string name) {
        var isDouble = name == "Vector" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES || name == "Vector4d";
        X = isDouble ? reader.Read<double>() : reader.Read<float>();
        Y = isDouble ? reader.Read<double>() : reader.Read<float>();
        Z = isDouble ? reader.Read<double>() : reader.Read<float>();
    }

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public Vector3D<double> ToSilk() => new(X, Y, Z);
}
