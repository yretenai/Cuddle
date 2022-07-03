using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration]
public record FOrientedBox : FFallbackStruct {
    public FOrientedBox(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "OrientedBox") { }

    public FOrientedBox(FArchiveReader reader, string name) {
        var isDouble = reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES;
        Center = new FVector(reader, "Vector");
        AxisX = new FVector(reader, "Vector");
        AxisY = new FVector(reader, "Vector");
        AxisZ = new FVector(reader, "Vector");
        ExtentX = isDouble ? reader.Read<double>() : reader.Read<float>();
        ExtentY = isDouble ? reader.Read<double>() : reader.Read<float>();
        ExtentZ = isDouble ? reader.Read<double>() : reader.Read<float>();
    }

    public FVector Center { get; set; }
    public FVector AxisX { get; set; }
    public FVector AxisY { get; set; }
    public FVector AxisZ { get; set; }
    public double ExtentX { get; set; }
    public double ExtentY { get; set; }
    public double ExtentZ { get; set; }
}
