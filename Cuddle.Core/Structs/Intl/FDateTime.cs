using Cuddle.Core.Assets;

namespace Cuddle.Core.Structs.Intl;

public record struct FDateTime(long Ticks) : FFallbackStruct {
    public FDateTime() : this(0) { }
}
