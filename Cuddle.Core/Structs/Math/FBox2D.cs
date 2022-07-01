using System.Numerics;
using System.Runtime.InteropServices;

namespace Cuddle.Core.Structs.Math;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x11)]
public readonly record struct FBox2D(Vector2 Min, Vector2 Max, bool IsValid) {
    public FBox2D() : this(Vector2.Zero, Vector2.Zero, false) { }
}
