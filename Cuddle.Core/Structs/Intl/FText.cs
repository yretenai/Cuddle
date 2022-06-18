using System;
using Cuddle.Core.Enums;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Intl;

public class FText {
    public FText(FArchiveReader data) {
        Flags = data.Read<ETextFlag>();
        if (data.Version >= EObjectVersion.FTEXT_HISTORY) {
            Type = data.Read<ETextHistoryType>();
            History = Type switch {
                ETextHistoryType.None => new FTextHistory.None(data),
                ETextHistoryType.Base => new FTextHistory.Base(data),
                ETextHistoryType.NamedFormat => new FTextHistory.NamedFormat(data),
                ETextHistoryType.OrderedFormat => new FTextHistory.OrderedFormat(data),
                ETextHistoryType.ArgumentFormat => new FTextHistory.ArgumentFormat(data),
                ETextHistoryType.AsNumber => new FTextHistory.AsNumber(data),
                ETextHistoryType.AsPercent => new FTextHistory.AsPercent(data),
                ETextHistoryType.AsCurrency => new FTextHistory.AsCurrency(data),
                ETextHistoryType.AsDate => new FTextHistory.AsDate(data),
                ETextHistoryType.AsTime => new FTextHistory.AsTime(data),
                ETextHistoryType.AsDateTime => new FTextHistory.AsDateTime(data),
                ETextHistoryType.Transform => new FTextHistory.Transform(data),
                ETextHistoryType.StringTableEntry => new FTextHistory.StringTableEntry(data),
                ETextHistoryType.TextGenerator => new FTextHistory.TextGenerator(data),
                _ => throw new NotSupportedException("History type is not supported"),
            };
        } else {
            Type = ETextHistoryType.Base;
            History = new FTextHistory.Base(data);
        }
    }

    public ETextFlag Flags { get; }
    public ETextHistoryType Type { get; }
    public FTextHistory History { get; }

    public override string ToString() => History.BuildDisplayString();
}
