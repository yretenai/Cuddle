namespace Cuddle.Core.Structs.Intl;

public record struct FDateTime(long Ticks) {
    public FDateTime() : this(0) { }
}
