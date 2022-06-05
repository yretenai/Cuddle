﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cuddle.Core.Assets;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core.Json;

public class UObjectSerializer : JsonConverter<UObject> {
    public override UObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, UObject value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WriteStartObject("Export");
        writer.WriteString("Guid", value.Guid);
        writer.WriteString("Name", value.Export.ObjectName.Value);
        writer.WriteString("Class", value.Export.ClassIndex.Reference?.ObjectName);
        writer.WriteString("ClassOwner", (value.Export.ClassIndex.Reference as FObjectImport)?.PackageIndex?.Reference?.ObjectName);
        writer.WriteString("Template", value.Export.TemplateIndex.Reference?.ObjectName);
        writer.WriteString("TemplateOwner", (value.Export.TemplateIndex.Reference as FObjectImport)?.PackageIndex?.Reference?.ObjectName);
        writer.WriteString("Outer", value.Export.OuterIndex.Reference?.ObjectName);
        writer.WriteString("OuterOwner", (value.Export.OuterIndex.Reference as FObjectImport)?.PackageIndex?.Reference?.ObjectName);
        writer.WriteString("Super", value.Export.SuperIndex.Reference?.ObjectName);
        writer.WriteString("SuperOwner", (value.Export.SuperIndex.Reference as FObjectImport)?.PackageIndex?.Reference?.ObjectName);
        writer.WriteEndObject();
        var isPureObject = value.GetType() == typeof(UObject);
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
            var element = JsonSerializer.SerializeToElement(value, value.GetType(), options);
            foreach (var elementData in element.EnumerateObject()) {
                writer.WritePropertyName(elementData.Name);
                writer.WriteRawValue(elementData.Value.GetRawText());
            }
        }

        writer.WriteEndObject();
    }
}
