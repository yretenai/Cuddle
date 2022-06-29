using System;
using System.Runtime.InteropServices;

namespace Cuddle.Core.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 5)]
public readonly struct Int40BE : IEquatable<Int40BE>, IEquatable<int>, IEquatable<long>, IFormattable {
    public readonly byte A;
    public readonly byte B;
    public readonly byte C;
    public readonly byte D;
    public readonly byte E;

    public static implicit operator long(Int40BE value) => value.ToInt64();
    public long ToInt64() => E | ((long)D << 8) | ((long)C << 16) | ((long)B << 24) | ((long)A << 32);

    public bool Equals(int other) => other == ToInt64();

    public bool Equals(long other) => other == ToInt64();
    public bool Equals(Int40BE other) => other.A == A && other.B == B && other.C == C && other.D == D && other.E == E;

    public override bool Equals(object? obj) =>
        obj switch {
            null => false,
            long l => l == ToInt64(),
            Int40BE i40 => Equals(i40),
            _ => false,
        };

    public override int GetHashCode() => ToInt64().GetHashCode();
    public override string ToString() => ToInt64().ToString();

    public string ToString(string? format, IFormatProvider? formatProvider) => ToInt64().ToString(format, formatProvider);

    public static bool operator ==(Int40BE left, Int40BE right) => left.Equals(right);

    public static bool operator !=(Int40BE left, Int40BE right) => !(left == right);
}
