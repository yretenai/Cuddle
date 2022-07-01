using System.Runtime.InteropServices;

namespace Cuddle.Core.Structs.Intl;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct FNumberFormattingOptions {
    public static FNumberFormattingOptions Default =>
        new() {
            AlwaysSign = false,
            UseGrouping = true,
            RoundingMode = ERoundingMode.HalfToEven,
            MinimumIntegralDigits = 1,
            MaximumIntegralDigits = 308 + 15 + 1,
            MinimumFractionalDigits = 0,
            MaximumFractionalDigits = 3,
        };

    public static FNumberFormattingOptions DefaultNoGrouping => Default with { UseGrouping = false };

#pragma warning disable CA1822
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public string Format(object? value) =>
        // todo: this is a lot of branching conditions and I don't have the patience to write a float formatter.
        value?.ToString() ?? "0";
#pragma warning restore CA1822

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
