using System;
using System.Collections.Generic;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Intl;

public abstract class FTextHistory {
    // todo: localization provider.
    public abstract string BuildDisplayString();

    public class None : FTextHistory {
        public None(FArchiveReader data) {
            if (data.Game.FindEditorVersion() >= EEditorObjectVersion.CultureInvariantTextSerializationKeyStability) {
                if (data.ReadBoolean()) {
                    SourceString = data.ReadString();
                }
            }
        }

        public string SourceString { get; } = "None";

        public override string BuildDisplayString() => SourceString;
    }

    public class Base : FTextHistory {
        public Base(FArchiveReader data) {
            if (data.Version < EObjectVersion.FTEXT_HISTORY) {
                SourceString = data.ReadString();
                Namespace = data.ReadString();
                Key = data.ReadString();
            } else {
                Namespace = data.ReadString();
                Key = data.ReadString();
                SourceString = data.ReadString();
            }
        }

        public string Namespace { get; }
        public string Key { get; }
        public string SourceString { get; }

        public override string BuildDisplayString() => SourceString.IsNullOrNone() ? $"{Namespace}_{Key}" : SourceString;
    }

    public class NamedFormat : FTextHistory {
        public NamedFormat(FArchiveReader data) {
            SourceFmt = new FText(data);
            Arguments = new Dictionary<string, FFormatArgumentValue>(data.Read<int>());
            for (var i = 0; i < Arguments.Count; i++) {
                Arguments[data.ReadString()] = new FFormatArgumentValue(data);
            }
        }

        public FText SourceFmt { get; }
        public Dictionary<string, FFormatArgumentValue> Arguments { get; }

        public override string BuildDisplayString() => SourceFmt.History.BuildDisplayString();
    }

    public class OrderedFormat : FTextHistory {
        public OrderedFormat(FArchiveReader data) {
            SourceFmt = new FText(data);
            Arguments = data.ReadClassArray<FFormatArgumentValue>();
        }

        public FText SourceFmt { get; }
        public FFormatArgumentValue[] Arguments { get; }

        public override string BuildDisplayString() => SourceFmt.History.BuildDisplayString();
    }

    public class ArgumentFormat : FTextHistory {
        public ArgumentFormat(FArchiveReader data) {
            SourceFmt = new FText(data);
            Arguments = new List<KeyValuePair<string, FFormatArgumentValue>>(data.Read<int>());
            for (var i = 0; i < Arguments.Count; i++) {
                Arguments.Add(new KeyValuePair<string, FFormatArgumentValue>(data.ReadString(), new FFormatArgumentValue(data)));
            }
        }

        public FText SourceFmt { get; }
        public List<KeyValuePair<string, FFormatArgumentValue>> Arguments { get; }

        public override string BuildDisplayString() => SourceFmt.History.BuildDisplayString();
    }

    public class AsNumber : FTextHistory {
        public AsNumber(FArchiveReader data) {
            SourceValue = new FFormatArgumentValue(data);
            if (data.ReadBoolean()) {
                FormatOptions = data.Read<FNumberFormattingOptions>();
            }

            TargetCulture = data.ReadString();
        }

        public FFormatArgumentValue SourceValue { get; }
        public FNumberFormattingOptions? FormatOptions { get; }
        public string TargetCulture { get; }

        public override string BuildDisplayString() => (FormatOptions ?? FNumberFormattingOptions.DefaultNoGrouping).Format(SourceValue.Value);
    }

    public class AsCurrency : FTextHistory {
        public AsCurrency(FArchiveReader data) {
            if (data.Version >= EObjectVersion.ADDED_CURRENCY_CODE_TO_FTEXT) {
                CurrencyCode = data.ReadString();
            }

            SourceValue = new FFormatArgumentValue(data);
            if (data.ReadBoolean()) {
                FormatOptions = data.Read<FNumberFormattingOptions>();
            }

            TargetCulture = data.ReadString();
        }

        public string CurrencyCode { get; } = "xx";
        public FFormatArgumentValue SourceValue { get; }
        public FNumberFormattingOptions? FormatOptions { get; }
        public string TargetCulture { get; }

        // todo: format currency, use simoleons for now.
        public override string BuildDisplayString() => "§" + (FormatOptions ?? FNumberFormattingOptions.DefaultNoGrouping).Format(SourceValue.Value);
    }

    public class AsPercent : FTextHistory {
        public AsPercent(FArchiveReader data) {
            SourceValue = new FFormatArgumentValue(data);
            if (data.ReadBoolean()) {
                FormatOptions = data.Read<FNumberFormattingOptions>();
            }

            TargetCulture = data.ReadString();
        }

        public FFormatArgumentValue SourceValue { get; }
        public FNumberFormattingOptions? FormatOptions { get; }
        public string TargetCulture { get; }

        public override string BuildDisplayString() => (FormatOptions ?? FNumberFormattingOptions.DefaultNoGrouping).Format(SourceValue.Value) + "%";
    }

    public class AsDate : FTextHistory {
        public AsDate(FArchiveReader data) {
            SourceDateTime = data.Read<FDateTime>();
            DateStyle = data.Read<EDateTimeStyle>();
            if (data.Version >= EObjectVersion.FTEXT_HISTORY_DATE_TIMEZONE) {
                TimeZone = data.ReadString();
            }

            TargetCulture = data.ReadString();
        }

        public FDateTime SourceDateTime { get; }
        public EDateTimeStyle DateStyle { get; }
        public string? TimeZone { get; }
        public string TargetCulture { get; }

        public override string BuildDisplayString() {
            var dt = new DateTime(SourceDateTime.Ticks);
            // todo: TimeZone, Culture
            return DateStyle switch {
                EDateTimeStyle.Default => dt.ToString("d/M/y"),
                EDateTimeStyle.Short => dt.ToString("d/M/y"),
                EDateTimeStyle.Medium => dt.ToString("MM d, y"),
                EDateTimeStyle.Long => dt.ToString("MMMM d, y"),
                EDateTimeStyle.Full => dt.ToString("DDD, MMM d, y"),
                EDateTimeStyle.Custom => dt.ToString("d/M/y"), // not supported, internal use only?
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public class AsTime : FTextHistory {
        public AsTime(FArchiveReader data) {
            SourceDateTime = data.Read<FDateTime>();
            DateStyle = data.Read<EDateTimeStyle>();
            TimeZone = data.ReadString();
            TargetCulture = data.ReadString();
        }

        public FDateTime SourceDateTime { get; }
        public EDateTimeStyle DateStyle { get; }
        public string TimeZone { get; }
        public string TargetCulture { get; }

        public override string BuildDisplayString() {
            var dt = new DateTime(SourceDateTime.Ticks);
            // todo: TimeZone, Culture
            return DateStyle switch {
                EDateTimeStyle.Default => dt.ToString("h:m tt"),
                EDateTimeStyle.Short => dt.ToString("h:m tt"),
                EDateTimeStyle.Medium => dt.ToString("h:mm:ss tt"),
                EDateTimeStyle.Long => dt.ToString("h:mm:ss tt zz"),
                EDateTimeStyle.Full => dt.ToString("y a\\t h:mm:ss tt zz"),
                EDateTimeStyle.Custom => dt.ToString("h:m tt"), // not supported, internal use only?
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public class AsDateTime : FTextHistory {
        public AsDateTime(FArchiveReader data) {
            SourceDateTime = data.Read<FDateTime>();
            DateStyle = data.Read<EDateTimeStyle>();
            TimeZone = data.ReadString();
            TargetCulture = data.ReadString();
        }

        public FDateTime SourceDateTime { get; }
        public EDateTimeStyle DateStyle { get; }
        public EDateTimeStyle TimeStyle { get; }
        public string TimeZone { get; }
        public string TargetCulture { get; }

        public override string BuildDisplayString() {
            var dt = new DateTime(SourceDateTime.Ticks);
            // todo: TimeZone, Culture
            return DateStyle switch {
                EDateTimeStyle.Default => dt.ToString("d/M/y, h:m tt"),
                EDateTimeStyle.Short => dt.ToString("d/M/y, h:m tt"),
                EDateTimeStyle.Medium => dt.ToString("MM d, y, h:mm:ss tt"),
                EDateTimeStyle.Long => dt.ToString("MMMM d, y, h:mm:ss tt zz"),
                EDateTimeStyle.Full => dt.ToString("DDD, MMM d, y a\\t h:mm:ss tt zz"),
                EDateTimeStyle.Custom => dt.ToString("d/M/y, h:m tt"), // not supported, internal use only?
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public class Transform : FTextHistory {
        public Transform(FArchiveReader data) {
            SourceText = new FText(data);
            TransformType = data.Read<ETransformType>();
        }

        public FText SourceText { get; }
        public ETransformType TransformType { get; }

        public override string BuildDisplayString() {
            var str = SourceText.History.BuildDisplayString();
            return TransformType switch {
                ETransformType.ToLower => str.ToLower(),
                ETransformType.ToUpper => str.ToUpper(),
                _ => str,
            };
        }
    }

    public class StringTableEntry : FTextHistory {
        public StringTableEntry(FArchiveReader data) {
            TableId = new FName(data);
            Key = data.ReadString();
        }

        public FName TableId { get; }
        public string Key { get; }

        // todo: lookup?
        public override string BuildDisplayString() => $"{TableId}_{Key}";
    }

    public class TextGenerator : FTextHistory {
        public TextGenerator(FArchiveReader data) {
            GeneratorTypeId = new FName(data);
            if (GeneratorTypeId.Value != "None") {
                GeneratorContents = data.ReadArray<byte>().ToArray();
            }
        }

        public FName GeneratorTypeId { get; }
        public byte[] GeneratorContents { get; } = Array.Empty<byte>();

        // all TextGenreator instances are game specific.
        public override string BuildDisplayString() => GeneratorTypeId;
    }
}
