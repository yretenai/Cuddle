using Cuddle.Core.Assets;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

public readonly record struct FIntPoint(int X, int Y) : FFallbackStruct {
    public FIntPoint() : this(0, 0) { }
    public Vector2D<int> ToSilk() => new(X, Y);
}
