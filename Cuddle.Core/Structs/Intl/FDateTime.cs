using Cuddle.Core.Assets;

namespace Cuddle.Core.Structs.Intl;

public record struct FDateTime(long Ticks) : FStructValue {
    public FDateTime() : this(0) { }
}
