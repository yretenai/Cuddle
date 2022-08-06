using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("Plane4f", "Plane4d", "Plane")]
public record FPlane : FFallbackStruct {
    public FPlane() { }
    public FPlane(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "Plane") { }

    public FPlane(FArchiveReader reader, string name) {
        var isDouble = name == "Plane" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES || name == "Plane4d";
        X = isDouble ? reader.Read<double>() : reader.Read<float>();
        Y = isDouble ? reader.Read<double>() : reader.Read<float>();
        Z = isDouble ? reader.Read<double>() : reader.Read<float>();
        W = isDouble ? reader.Read<double>() : reader.Read<float>();
    }

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double W { get; set; }

    public Plane<double> ToSilk() => new(X, Y, Z, W);
    public Vector4D<double> ToSilkDouble() => new(X, Y, Z, W);
}
