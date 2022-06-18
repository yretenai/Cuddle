using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cuddle.Core.Structs.Intl;

namespace Cuddle.Core.Json;

public class FTextSerializer : JsonConverter<FText> {
    public override FText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();
    public override void Write(Utf8JsonWriter writer, FText value, JsonSerializerOptions options) => writer.WriteStringValue(value.History.BuildDisplayString());
    public override void WriteAsPropertyName(Utf8JsonWriter writer, FText value, JsonSerializerOptions options) => writer.WritePropertyName(value.History.BuildDisplayString());
}
