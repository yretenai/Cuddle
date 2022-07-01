using Cuddle.Core.Assets;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core.Structs.Math;

public record FCapsuleShape : FTaggedStructValue {
    public FCapsuleShape() : this(new FVector(), 0f, new FVector(), 0f) { }

    public FCapsuleShape(FPropertyOwner owner) => Owner = owner;

    public FCapsuleShape(FVector center, float radius, FVector orientation, float length) {
        Center = center;
        Radius = radius;
        Orientation = orientation;
        Length = length;
    }

    public FVector Center { get; set; } = new();
    public float Radius { get; set; }
    public FVector Orientation { get; set; } = new();
    public float Length { get; set; }
    internal override bool SerializeProperties => false;

    public override void ProcessProperties(FPropertyOwner owner) {
        Center = owner.GetProperty<FVector>(nameof(Center)) ?? Center;
        Radius = owner.GetProperty<float>(nameof(Radius));
        Orientation = owner.GetProperty<FVector>(nameof(Orientation)) ?? Orientation;
        Length = owner.GetProperty<float>(nameof(Length));
    }

    public void Deconstruct(out FVector center, out float radius, out FVector orientation, out float length) {
        center = Center;
        radius = Radius;
        orientation = Orientation;
        length = Length;
    }
}

// todo: FKShapeElem, FKBoxElem, FKConvexElem, FKSphereElem, FKSphylElem, FKTaperedCapsuleElem
