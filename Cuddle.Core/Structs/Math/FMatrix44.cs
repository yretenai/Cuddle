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
        XPlane = new FPlane(reader, planeName);
        YPlane = new FPlane(reader, planeName);
        ZPlane = new FPlane(reader, planeName);
        WPlane = new FPlane(reader, planeName);
    }

    public FPlane XPlane { get; set; }
    public FPlane YPlane { get; set; }
    public FPlane ZPlane { get; set; }
    public FPlane WPlane { get; set; }
    public Matrix4X4<double> ToSilk() => new(XPlane.ToSilk(), YPlane.ToSilk(), ZPlane.ToSilk(), WPlane.ToSilk());
}
