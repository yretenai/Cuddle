using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cuddle.Core.Assets;

namespace Cuddle.Core.Json;

public class FTaggedStructValueSerializer : JsonConverter<FTaggedStructValue> {
    public override FTaggedStructValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, FTaggedStructValue value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        var isPureObject = value.GetType() == typeof(FTaggedStructValue);
        if (value.SerializeProperties) {
            if (!isPureObject) {
                writer.WriteStartObject("Properties");
            }

            foreach (var (tag, property) in value.Properties) {
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

        if (!isPureObject) {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }

        writer.WriteEndObject();
    }
}
