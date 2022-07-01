using System;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Intl;

public record FFormatArgumentValue {
    public FFormatArgumentValue() => Type = EFormatArgumentType.None;

    public FFormatArgumentValue(FArchiveReader data) {
        Type = data.Read<EFormatArgumentType>();
        Value = Type switch {
            EFormatArgumentType.Text => new FText(data),
            EFormatArgumentType.Int => data.Read<long>(),
            EFormatArgumentType.UInt => data.Read<ulong>(),
            EFormatArgumentType.Double => data.Read<double>(),
            EFormatArgumentType.Float => data.Read<float>(),
            EFormatArgumentType.Gender => data.Read<ETextGender>(),
            EFormatArgumentType.None => null,
            _ => throw new NotSupportedException("Format argument type is not supported"),
        };
    }

    public EFormatArgumentType Type { get; }
    public object? Value { get; }
}
