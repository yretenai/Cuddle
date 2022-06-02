using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cuddle.Core.Assets;

namespace Cuddle.Core.Json;

public class UPropertySerializerFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(UProperty));
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => new Serializer();

    private class Serializer : JsonConverter<UProperty> {
        public override UProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

        public override void Write(Utf8JsonWriter writer, UProperty? value, JsonSerializerOptions options) {
            var innerValue = value?.GetType().GetProperty("Value")?.GetValue(value);
            if (innerValue == null) {
                writer.WriteNullValue();
            } else {
                JsonSerializer.Serialize(writer, innerValue, innerValue.GetType(), options);
            }
        }
    }
}
