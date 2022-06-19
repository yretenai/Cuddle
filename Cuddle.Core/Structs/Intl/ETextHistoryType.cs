namespace Cuddle.Core.Structs.Intl;

public enum ETextHistoryType : sbyte {
    None = -1,
    Base = 0,
    NamedFormat,
    OrderedFormat,
    ArgumentFormat,
    AsNumber,
    AsPercent,
    AsCurrency,
    AsDate,
    AsTime,
    AsDateTime,
    Transform,
    StringTableEntry,
    TextGenerator,
}
