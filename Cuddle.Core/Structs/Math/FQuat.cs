using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("Quat4f", "Quat4d", "Quat")]
public record FQuat : FFallbackStruct {
    public FQuat() { }
    public FQuat(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "Quat") { }

    public FQuat(FArchiveReader reader, string name) {
        var isDouble = name == "Quat" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES || name == "Quat4d";
        X = isDouble ? reader.Read<double>() : reader.Read<float>();
        Y = isDouble ? reader.Read<double>() : reader.Read<float>();
        Z = isDouble ? reader.Read<double>() : reader.Read<float>();
        W = isDouble ? reader.Read<double>() : reader.Read<float>();
    }

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double W { get; set; }

    public Quaternion<double> ToSilk() => new(X, Y, Z, W);
}
