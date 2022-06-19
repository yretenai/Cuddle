namespace Cuddle.Core.Structs.Intl;

public enum ETextFlag : uint {
    Transient = 1,
    CultureInvariant = 2,
    ConvertedProperty = 4,
    Immutable = 8,
    InitializedFromString = 16,
}
