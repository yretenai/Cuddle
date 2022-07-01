using System.Numerics;
using System.Runtime.InteropServices;

namespace Cuddle.Core.Structs.Math;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x1C)]
public readonly record struct FBoxSphereBounds(Vector3 Origin, Vector3 BoxExtent, float SphereRadius) {
    public FBoxSphereBounds() : this(Vector3.Zero, Vector3.Zero, 0f) { }
}
