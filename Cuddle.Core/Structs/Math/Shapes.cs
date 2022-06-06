using System.Numerics;
using System.Runtime.InteropServices;
using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Math;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x1C)]
public record FCapsuleShape : FTaggedStructValue {
    public FCapsuleShape() : this(Vector3.Zero, 0f, Vector3.Zero, 0f) { }

    public FCapsuleShape(FArchiveReader data, FPropertyTagContext context, FName name) : base(data, context, name) {
        Center = GetProperty<Vector3>(new FName(nameof(Center)));
        Radius = GetProperty<float>(new FName(nameof(Radius)));
        Orientation = GetProperty<Vector3>(new FName(nameof(Orientation)));
        Length = GetProperty<float>(new FName(nameof(Length)));
    }

    public FCapsuleShape(Vector3 center, float radius, Vector3 orientation, float length) {
        Center = center;
        Radius = radius;
        Orientation = orientation;
        Length = length;
    }

    public Vector3 Center { get; init; }
    public float Radius { get; init; }
    public Vector3 Orientation { get; init; }
    public float Length { get; init; }

    public void Deconstruct(out Vector3 center, out float radius, out Vector3 orientation, out float length) {
        center = Center;
        radius = Radius;
        orientation = Orientation;
        length = Length;
    }
}

// todo: FKShapeElem, FKBoxElem, FKConvexElem, FKSphereElem, FKSphylElem, FKTaperedCapsuleElem
