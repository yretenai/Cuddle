using System;
using System.Runtime.InteropServices;

namespace Cuddle.Core.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
public readonly struct Int24 : IEquatable<Int24>, IEquatable<int>, IFormattable {
    public readonly byte A;
    public readonly byte B;
    public readonly byte C;

    public static implicit operator int(Int24 value) => value.ToInt32();
    public int ToInt32() => A | (B << 8) | (C << 16);

    public bool Equals(int other) => other == ToInt32();

    public bool Equals(long other) => other == ToInt32();
    public bool Equals(Int24 other) => other.A == A && other.B == B && other.C == C;

    public override bool Equals(object? obj) =>
        obj switch {
            null => false,
            int i => i == ToInt32(),
            Int24 i20 => Equals(i20),
            _ => false,
        };

    public override int GetHashCode() => ToInt32().GetHashCode();
    public override string ToString() => ToInt32().ToString();

    public string ToString(string? format, IFormatProvider? formatProvider) => ToInt32().ToString(format, formatProvider);

    public static bool operator ==(Int24 left, Int24 right) => left.Equals(right);

    public static bool operator !=(Int24 left, Int24 right) => !(left == right);
}
