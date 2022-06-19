using System.Numerics;
using Cuddle.Core.Assets;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core.Structs.Math;

public record FCapsuleShape : FTaggedStructValue {
    public FCapsuleShape() : this(Vector3.Zero, 0f, Vector3.Zero, 0f) { }

    public FCapsuleShape(FPropertyOwner owner) => Owner = owner;

    public FCapsuleShape(Vector3 center, float radius, Vector3 orientation, float length) {
        Center = center;
        Radius = radius;
        Orientation = orientation;
        Length = length;
    }

    public Vector3 Center { get; set; }
    public float Radius { get; set; }
    public Vector3 Orientation { get; set; }
    public float Length { get; set; }
    internal override bool SerializeProperties => false;

    public override void ProcessProperties(FPropertyOwner owner) {
        Center = owner.GetProperty<Vector3>(nameof(Center));
        Radius = owner.GetProperty<float>(nameof(Radius));
        Orientation = owner.GetProperty<Vector3>(nameof(Orientation));
        Length = owner.GetProperty<float>(nameof(Length));
    }

    public void Deconstruct(out Vector3 center, out float radius, out Vector3 orientation, out float length) {
        center = Center;
        radius = Radius;
        orientation = Orientation;
        length = Length;
    }
}

// todo: FKShapeElem, FKBoxElem, FKConvexElem, FKSphereElem, FKSphylElem, FKTaperedCapsuleElem
