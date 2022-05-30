namespace Cuddle.Core.Assets;

public readonly record struct FPropertyTagContext(FPropertyTag ElementTag, FPropertyReadMode ReadMode, bool IsGVAS) {
    public static FPropertyTagContext Empty => new(FPropertyTag.Empty, FPropertyReadMode.Normal, false);
    public static FPropertyTagContext GVAS => new(FPropertyTag.Empty, FPropertyReadMode.Normal, true);
}
