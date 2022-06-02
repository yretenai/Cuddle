using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core.Json;

public class FPackageIndexSerializer : JsonConverter<FPackageIndex> {
    public override FPackageIndex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, FPackageIndex value, JsonSerializerOptions options) {
        var str = value.Reference?.ObjectName.Value;
        if (str == null) {
            writer.WriteNullValue();
        } else {
            writer.WriteStringValue(str);
        }
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, FPackageIndex value, JsonSerializerOptions options) => writer.WritePropertyName(value.Reference!.ObjectName.Value);
}
