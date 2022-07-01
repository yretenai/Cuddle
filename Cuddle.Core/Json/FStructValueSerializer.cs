using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cuddle.Core.Assets;

namespace Cuddle.Core.Json;

public class FStructValueSerializer : JsonConverter<FFallbackStruct> {
    public override FFallbackStruct Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, FFallbackStruct value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        if (value is FTaggedStructValue { SerializeProperties: true } taggedStructValue) {
            var isPureObject = value.GetType() == typeof(FTaggedStructValue);
            if (!isPureObject) {
                writer.WriteStartObject("Properties");
            }

            foreach (var (tag, property) in taggedStructValue.Properties) {
                writer.WritePropertyName(tag.Index > 0 ? $"{tag.Name.Value}.{tag.Index}" : tag.Name.Value);
                if (property == null) {
                    writer.WriteNullValue();
                } else {
                    JsonSerializer.Serialize(writer, property, property.GetType(), options);
                }
            }

            if (!isPureObject) {
                writer.WriteEndObject();
            }
        }

        var element = JsonSerializer.SerializeToElement(value, value.GetType(), options);
        foreach (var elementData in element.EnumerateObject()) {
            writer.WritePropertyName(elementData.Name);
            writer.WriteRawValue(elementData.Value.GetRawText());
        }

        writer.WriteEndObject();
    }
}
