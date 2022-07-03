using Cuddle.Core.Assets;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core.Structs.Math;

public record FCapsuleShape : FTaggedStructValue {
    public FCapsuleShape() : this(new FVector(), 0f, new FVector(), 0f) { }

    public FCapsuleShape(FPropertyOwner owner) => Owner = owner;

    public FCapsuleShape(FVector center, double radius, FVector orientation, double length) {
        Center = center;
        Radius = radius;
        Orientation = orientation;
        Length = length;
    }

    public FVector Center { get; set; } = new();
    public double Radius { get; set; }
    public FVector Orientation { get; set; } = new();
    public double Length { get; set; }
    internal override bool SerializeProperties => false;

    public override void ProcessProperties(FPropertyOwner owner, EObjectVersion version, EObjectVersionUE5 versionUE5) {
        Center = owner.GetProperty<FVector>(nameof(Center)) ?? Center;
        Radius = versionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES ? owner.GetProperty<double>(nameof(Radius)) : owner.GetProperty<float>(nameof(Radius));
        Orientation = owner.GetProperty<FVector>(nameof(Orientation)) ?? Orientation;
        Length = versionUE5 >= EObjectVersionUE5.LARGE_WORLD_COORDINATES ? owner.GetProperty<double>(nameof(Length)) : owner.GetProperty<float>(nameof(Length));
    }

    public void Deconstruct(out FVector center, out double radius, out FVector orientation, out double length) {
        center = Center;
        radius = Radius;
        orientation = Orientation;
        length = Length;
    }
}

// todo: FKShapeElem, FKBoxElem, FKConvexElem, FKSphereElem, FKSphylElem, FKTaperedCapsuleElem
