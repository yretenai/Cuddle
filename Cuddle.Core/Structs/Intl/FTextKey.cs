using Cuddle.Core.Assets;

namespace Cuddle.Core.Structs.Intl;

public record struct FTextKey(uint Hash, string Value) : FFallbackStruct {
    public static implicit operator string(FTextKey key) => key.Value;
}
