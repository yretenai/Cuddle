using Cuddle.Core.Assets;
using Silk.NET.Maths;

namespace Cuddle.Core.Structs.Math;

public readonly record struct FIntVector(int X, int Y, int Z) : FFallbackStruct {
    public FIntVector() : this(0, 0, 0) { }
    public Vector3D<int> ToSilk() => new(X, Y, Z);
}
