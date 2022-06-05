namespace Cuddle.Core.Assets;

public readonly record struct FPropertyTagContext(FPropertyTag ElementTag, FPropertyTag? ContextTag, FPropertyReadMode ReadMode, bool IsGVAS) {
    public static FPropertyTagContext Empty => new(FPropertyTag.Empty, null, FPropertyReadMode.Normal, false);
    public static FPropertyTagContext GVAS => new(FPropertyTag.Empty, null, FPropertyReadMode.Normal, true);
}
