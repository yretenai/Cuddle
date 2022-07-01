namespace Cuddle.Core.Structs.Math;

public readonly record struct FIntVector(int X, int Y, int Z) {
    public FIntVector() : this(0, 0, 0) { }
}
