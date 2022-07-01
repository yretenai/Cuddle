using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration("Vector3f"), ObjectRegistration("Vector", MaxVersionUE5 = EObjectVersionUE5.LARGE_WORLD_COORDINATES)]
public record struct FVector3f(float X, float Y, float Z) {
    public static implicit operator FVector3d(FVector3f value) => new(value.X, value.Y, value.Z);

    public FVector3d ToFVector3d() => new(X, Y, Z);
}

[ObjectRegistration("Vector3d", "Vector")]
public record struct FVector3d(double X, double Y, double Z) {
    public static implicit operator FVector3f(FVector3d value) => new((float) value.X, (float) value.Y, (float) value.Z);

    public FVector3f ToFVector3f() => new((float) X, (float) Y, (float) Z);
}

[ObjectRegistration("Vector4f", "Plane4f", "Quat4f"), ObjectRegistration("Vector4", "Plane", "Quat", MaxVersionUE5 = EObjectVersionUE5.LARGE_WORLD_COORDINATES)]
public record struct FVector4f(float X, float Y, float Z, float W) {
    public static implicit operator FVector4d(FVector4f value) => new(value.X, value.Y, value.Z, value.W);

    public FVector4d ToFVector4d() => new(X, Y, Z, W);
}

[ObjectRegistration("Vector4d", "Vector4", "Plane4d", "Plane", "Quat4d", "Quat")]
public record struct FVector4d(double X, double Y, double Z, double W) {
    public static implicit operator FVector4f(FVector4d value) => new((float) value.X, (float) value.Y, (float) value.Z, (float) value.W);

    public FVector4f ToFVector4f() => new((float) X, (float) Y, (float) Z, (float) W);
}

[ObjectRegistration("Vector2f"), ObjectRegistration("Vector2D", MaxVersionUE5 = EObjectVersionUE5.LARGE_WORLD_COORDINATES)]
public record struct FVector2f(float X, float Y) {
    public static implicit operator FVector2d(FVector2f value) => new(value.X, value.Y);

    public FVector2d ToFVector2d() => new(X, Y);
}

[ObjectRegistration("Vector2D")]
public record struct FVector2d(double X, double Y) {
    public static implicit operator FVector2f(FVector2d value) => new((float) value.X, (float) value.Y);

    public FVector2f ToFVector2f() => new((float) X, (float) Y);
}

[ObjectRegistration("TwoVectors", MaxVersionUE5 = EObjectVersionUE5.LARGE_WORLD_COORDINATES)]
public record struct FTwoVectorsF(FVector2f V1, FVector2f V2);

[ObjectRegistration("TwoVectors")]
public record struct FTwoVectorsD(FVector2d V1, FVector2d V2);

[ObjectRegistration("Rotator3f"), ObjectRegistration("Rotator", MaxVersionUE5 = EObjectVersionUE5.LARGE_WORLD_COORDINATES)]
public record struct FRotator3f(float Pitch, float Yaw, float Roll) {
    public static implicit operator FRotator3d(FRotator3f value) => new(value.Pitch, value.Yaw, value.Roll);

    public FRotator3d ToFRotator3d() => new(Pitch, Yaw, Roll);
}

[ObjectRegistration("Rotator3d", "Rotator")]
public record struct FRotator3d(double Pitch, double Yaw, double Roll) {
    public static implicit operator FRotator3f(FRotator3d value) => new((float) value.Pitch, (float) value.Yaw, (float) value.Roll);

    public FRotator3f ToFRotator3f() => new((float) Pitch, (float) Yaw, (float) Roll);
}
