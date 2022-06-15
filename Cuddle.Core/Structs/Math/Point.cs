using Cuddle.Core.Assets;

namespace Cuddle.Core.Structs.Math; 

public readonly record struct FIntPoint(int X, int Y) : FStructValue {
    public FIntPoint() : this(0, 0) { }
}
