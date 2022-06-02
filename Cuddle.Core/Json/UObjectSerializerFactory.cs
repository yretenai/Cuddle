using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cuddle.Core.Assets;

namespace Cuddle.Core.Json;

public class UObjectSerializer : JsonConverter<UObject> {
    public override UObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, UObject value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WriteStartObject("Export");
        writer.WriteString("Guid", value.Guid);
        writer.WriteString("Name", value.Export.ObjectName.Value);
        writer.WriteString("Class", value.Export.ClassIndex.Reference?.ObjectName);
        writer.WriteString("Template", value.Export.TemplateIndex.Reference?.ObjectName);
        writer.WriteString("Outer", value.Export.OuterIndex.Reference?.ObjectName);
        writer.WriteString("Super", value.Export.SuperIndex.Reference?.ObjectName);
        writer.WriteEndObject();
        if (value.SerializeProperties) {
            writer.WriteStartObject("Properties");
            foreach (var (tag, property) in value.Properties) {
                writer.WritePropertyName(tag.Index > 0 ? $"{tag.Name.Value}.{tag.Index}" : tag.Name.Value);
                if (property == null) {
                    writer.WriteNullValue();
                } else {
                    JsonSerializer.Serialize(writer, property, property.GetType(), options);
                }
            }

            writer.WriteEndObject();
        }

        JsonSerializer.Serialize(writer, value, value.GetType(), options);
        writer.WriteEndObject();
    }
}
