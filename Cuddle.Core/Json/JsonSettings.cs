﻿using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cuddle.Core.Json;

public static class JsonSettings {
    public static JsonSerializerOptions Options =>
        new() {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = {
                new FNameSerializer(),
                new FPackageIndexSerializer(),
                new FPropertyTagSerializerFactory(),
                new FStructValueSerializer(),
                new FTaggedStructValueSerializer(),
                new FTextSerializer(),
                new UObjectSerializer(),
                new UPropertySerializerFactory(),
            },
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
}
