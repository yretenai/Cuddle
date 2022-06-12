using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cuddle.Core.Assets;

namespace Cuddle.Core.Json;

public class FPropertyTagSerializerFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(FPropertyTag));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => new Serializer();

    private class Serializer : JsonConverter<FPropertyTag> {
        public override FPropertyTag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();
        public override void Write(Utf8JsonWriter writer, FPropertyTag value, JsonSerializerOptions options) => writer.WriteStringValue(value.Index > 0 ? $"{value.Name.Value}.{value.Index}" : value.Name.Value);
        public override void WriteAsPropertyName(Utf8JsonWriter writer, FPropertyTag value, JsonSerializerOptions options) => writer.WritePropertyName(value.Index > 0 ? $"{value.Name.Value}.{value.Index}" : value.Name.Value);
    }
}
