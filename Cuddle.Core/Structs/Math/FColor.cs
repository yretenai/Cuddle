using Cuddle.Core.Assets;

namespace Cuddle.Core.Structs.Math;

public readonly record struct FColor(byte R, byte G, byte B, byte A) : FStructValue {
    public FColor() : this(0, 0, 0, 0) { }
}
