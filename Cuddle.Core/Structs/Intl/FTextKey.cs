namespace Cuddle.Core.Structs.Intl;

public record struct FTextKey(uint Hash, string Value) {
    public static implicit operator string(FTextKey key) {
        return key.Value;
    }
}