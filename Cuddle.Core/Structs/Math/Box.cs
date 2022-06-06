using System.Numerics;
using System.Runtime.InteropServices;
using Cuddle.Core.Assets;

namespace Cuddle.Core.Structs.Math;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x19)]
public readonly record struct FBox(Vector3 Min, Vector3 Max, bool IsValid) : FStructValue {
    public FBox() : this(Vector3.Zero, Vector3.Zero, false) { }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x11)]
public readonly record struct FBox2D(Vector2 Min, Vector2 Max, bool IsValid) : FStructValue {
    public FBox2D() : this(Vector2.Zero, Vector2.Zero, false) { }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x1C)]
public readonly record struct FBoxSphereBounds(Vector3 Origin, Vector3 BoxExtent, float SphereRadius) : FStructValue {
    public FBoxSphereBounds() : this(Vector3.Zero, Vector3.Zero, 0f) { }
}
