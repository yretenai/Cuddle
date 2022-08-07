using Cuddle.Core.Assets;
using Cuddle.Core.Objects;

namespace Cuddle.Core.Structs.Intl;

[ObjectRegistration("DateTime", "Timespan")]
public record struct FDateTime(long Ticks) : FFallbackStruct {
    public FDateTime() : this(0) { }
}
