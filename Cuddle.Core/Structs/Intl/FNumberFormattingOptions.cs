using System.Runtime.InteropServices;
using Cuddle.Core.Assets;
using Cuddle.Core.Enums;

namespace Cuddle.Core.Structs.Intl;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct FNumberFormattingOptions : FStructValue {
    public string Format(object? value) =>
        // todo: this is a lot of branching conditions and I don't have the patience to write a float formatter.
        value?.ToString() ?? "0";

    // @formatter:off
    [MarshalAs(UnmanagedType.I4)] public bool AlwaysSign;
    [MarshalAs(UnmanagedType.I4)] public bool UseGrouping;
    public ERoundingMode RoundingMode;
    public int MinimumIntegralDigits;
    public int MaximumIntegralDigits;
    public int MinimumFractionalDigits;
    public int MaximumFractionalDigits;
    // @formatter:on
}
