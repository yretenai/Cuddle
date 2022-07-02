using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

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
}

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
}

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
}

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
}

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
}

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
}

[ObjectRegistration("TwoVectors")]
public record FTwoVectors : FFallbackStruct {
    public FTwoVectors() {
        V1 = new FVector();
        V2 = new FVector();
    }

    public FTwoVectors(FArchiveReader reader) {
        V1 = new FVector(reader, "Vector");
        V2 = new FVector(reader, "Vector");
    }

    public FVector V1 { get; set; }
    public FVector V2 { get; set; }
}
