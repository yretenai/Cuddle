using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("FBox3f", "FBox3d", "FBox")]
public record FBox : FFallbackStruct {
    public FBox(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "FBox") { }

    public FBox(FArchiveReader reader, string name) {
        var isDouble = name == "FBox" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES || name == "FBox3d";
        var vectorName = isDouble ? "Vector3d" : "Vector3f";
        Origin = new FVector(reader, vectorName);
        BoxExtent = new FVector(reader, vectorName);
        IsValid = reader.ReadBoolean();
    }

    public FVector Origin { get; set; }
    public FVector BoxExtent { get; set; }
    public bool IsValid { get; set; }
}

[ObjectRegistration("FBox2f", "FBox2D")]
public record FBox2D : FFallbackStruct {
    public FBox2D(FArchiveReader reader) : this(reader, FStructRegistry.CurrentProcessingStruct.Value ?? "FBox2D") { }

    public FBox2D(FArchiveReader reader, string name) {
        var isDouble = name == "FBox2D" && reader.VersionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES;
        var vectorName = isDouble ? "Vector2D" : "Vector2f";
        Origin = new FVector2D(reader, vectorName);
        BoxExtent = new FVector2D(reader, vectorName);
        IsValid = reader.ReadBoolean();
    }

    public FVector2D Origin { get; set; }
    public FVector2D BoxExtent { get; set; }
    public bool IsValid { get; set; }
}
