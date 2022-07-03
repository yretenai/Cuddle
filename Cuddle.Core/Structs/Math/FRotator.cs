using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("Rotator3f", "Rotator3d", "Rotator")]
public record FRotator : FFallbackStruct {
    public FRotator() { }
    public FRotator(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "Rotator") { }

    public FRotator(FArchiveReader reader, string name) {
        var isDouble = name == "Rotator" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES || name == "Rotator3d";
        Pitch = isDouble ? reader.Read<double>() : reader.Read<float>();
        Yaw = isDouble ? reader.Read<double>() : reader.Read<float>();
        Roll = isDouble ? reader.Read<double>() : reader.Read<float>();
    }

    public double Pitch { get; set; }
    public double Yaw { get; set; }
    public double Roll { get; set; }

    public Vector3D<double> ToSilk() {
        var pr = Pitch * (System.Math.PI / 180.0f);
        var yr = Yaw * (System.Math.PI / 180.0d);
        var sp = System.Math.Sin(pr);
        var cp = System.Math.Cos(pr);
        var sy = System.Math.Sin(yr);
        var cy = System.Math.Cos(yr);

        return new Vector3D<double>(cp * cy, cp * sy, sp);
    }

    public Quaternion<double> ToSilkQ() {
        const double DIVIDE_BY_2 = System.Math.PI / 180.0d / 2.0d;
        var sp = System.Math.Sin(Pitch * DIVIDE_BY_2);
        var cp = System.Math.Cos(Pitch * DIVIDE_BY_2);
        var sy = System.Math.Sin(Yaw * DIVIDE_BY_2);
        var cy = System.Math.Cos(Yaw * DIVIDE_BY_2);
        var sr = System.Math.Sin(Roll * DIVIDE_BY_2);
        var cr = System.Math.Cos(Roll * DIVIDE_BY_2);

        return new Quaternion<double> {
            X = cr * sp * sy - sr * cp * cy,
            Y = -cr * sp * cy - sr * cp * sy,
            Z = cr * cp * sy - sr * sp * cy,
            W = cr * cp * cy + sr * sp * sy,
        };
    }
}
