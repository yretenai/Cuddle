using System;
using System.Runtime.InteropServices;

namespace Cuddle.Core.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 5)]
public readonly struct Int40 : IEquatable<Int40>, IEquatable<int>, IEquatable<long> {
    public readonly byte A;
    public readonly byte B;
    public readonly byte C;
    public readonly byte D;
    public readonly byte E;

    public static implicit operator long(Int40 value) => value.ToInt64();
    public long ToInt64() => A | ((long)B << 8) | ((long)C << 16) | ((long)D << 24) | ((long)E << 32);

    public bool Equals(int other) => other == ToInt64();

    public bool Equals(long other) => other == ToInt64();
    public bool Equals(Int40 other) => other.A == A && other.B == B && other.C == C && other.D == D && other.E == E;

    public override bool Equals(object? obj) =>
        obj switch {
            null => false,
            long l => l == ToInt64(),
            Int40 i40 => Equals(i40),
            _ => false,
        };

    public override int GetHashCode() => ToInt64().GetHashCode();

    public static bool operator ==(Int40 left, Int40 right) => left.Equals(right);

    public static bool operator !=(Int40 left, Int40 right) => !(left == right);
}
