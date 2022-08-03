using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cuddle.Core.Structs;

namespace Cuddle.Core.Json;

public class FNameSerializer : JsonConverter<FName> {
    public override FName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();
    public override void Write(Utf8JsonWriter writer, FName value, JsonSerializerOptions options) => writer.WriteStringValue(value.InstanceValue);
    public override void WriteAsPropertyName(Utf8JsonWriter writer, FName value, JsonSerializerOptions options) => writer.WritePropertyName(value.InstanceValue);
}
