using System;
using System.Runtime.InteropServices;

namespace Cuddle.Core.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 5)]
public readonly struct Int20 : IEquatable<Int20>, IEquatable<int> {
    public readonly byte A;
    public readonly byte B;
    public readonly byte C;

    public static implicit operator int(Int20 value) => value.ToInt32();
    public int ToInt32() => A | (B << 8) | (C << 16);

    public bool Equals(int other) => other == ToInt32();

    public bool Equals(long other) => other == ToInt32();
    public bool Equals(Int20 other) => other.A == A && other.B == B && other.C == C;

    public override bool Equals(object? obj) =>
        obj switch {
            null => false,
            int i => i == ToInt32(),
            Int20 i20 => Equals(i20),
            _ => false,
        };

    public override int GetHashCode() => ToInt32().GetHashCode();

    public static bool operator ==(Int20 left, Int20 right) => left.Equals(right);

    public static bool operator !=(Int20 left, Int20 right) => !(left == right);
}
