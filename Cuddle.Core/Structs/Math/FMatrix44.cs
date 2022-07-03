using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("FMatrix44f", "FMatrix44d", "FMatrix44")]
public record FMatrix44 : FFallbackStruct {
    public FMatrix44(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "Matrix44") { }

    public FMatrix44(FArchiveReader reader, string name) {
        var isDouble = name == "Matrix44" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES || name == "Matrix44d";
        var planeName = isDouble ? "Plane4d" : "Plane4f";
        XPlane = new FVector4(reader, planeName);
        YPlane = new FVector4(reader, planeName);
        ZPlane = new FVector4(reader, planeName);
        WPlane = new FVector4(reader, planeName);
    }

    public FVector4 XPlane { get; set; }
    public FVector4 YPlane { get; set; }
    public FVector4 ZPlane { get; set; }
    public FVector4 WPlane { get; set; }
    public Matrix4X4<double> ToSilk() => new(XPlane.ToSilk(), YPlane.ToSilk(), ZPlane.ToSilk(), WPlane.ToSilk());
}
