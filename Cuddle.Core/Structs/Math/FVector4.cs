using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("Vector4f", "Vector4d", "Vector4")]
public record FVector4 : FFallbackStruct {
    public FVector4() { }
    public FVector4(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "Vector4") { }

    public FVector4(FArchiveReader reader, string name) {
        var isDouble = name == "Vector4" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES || name == "Vector4d";
        X = isDouble ? reader.Read<double>() : reader.Read<float>();
        Y = isDouble ? reader.Read<double>() : reader.Read<float>();
        Z = isDouble ? reader.Read<double>() : reader.Read<float>();
        W = isDouble ? reader.Read<double>() : reader.Read<float>();
    }

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double W { get; set; }
    public Vector4D<double> ToSilk() => new(X, Y, Z, W);
}
