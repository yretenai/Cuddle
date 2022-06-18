using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using Cuddle.Core.Assets;
using Cuddle.Core.Structs.Property;

namespace Cuddle.Core.Structs.Asset;

public record PropertyOwner {
    [JsonIgnore]
    public Dictionary<FPropertyTag, UProperty?> Properties { get; protected init; } = new();

    public T? GetProperty<T>(FName name, T? defaultValue = default) => GetProperty(name.Value, name.Instance, defaultValue);

    public T? GetProperty<T>(string name, int instance = 0, T? defaultValue = default) => !TryGetProperty<T>(name, instance, out var value) ? defaultValue : value;

    public bool TryGetProperty<T>(FName name, [MaybeNullWhen(false)] out T v) {
        var properties = Properties.Where(x => x.Key.Name == name).ToArray();
        var property = default(UProperty);
        var t = typeof(T);
        switch (properties.Length) {
            case 0:
                v = default;
                return false;
            case 1:
                property = properties[0].Value;
                break;
            default: {
                if (t.IsAssignableTo(typeof(UProperty))) {
                    foreach (var prop in properties) {
                        if (prop.Value is T) {
                            property = prop.Value;
                            break;
                        }
                    }
                } else if (t.IsValueType || typeof(T).IsAssignableTo(typeof(FStructValue))) {
                    foreach (var prop in properties) {
                        if (prop.Value is StructProperty) {
                            property = prop.Value;
                        }
                    }
                } else if (t.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(t.GetGenericTypeDefinition())) {
                    foreach (var prop in properties) {
                        if (prop.Value is MapProperty) {
                            property = prop.Value;
                        }
                    }
                } else if (t.IsArray) {
                    foreach (var prop in properties) {
                        if (prop.Value is ArrayProperty or SetProperty) {
                            property = prop.Value;
                        }
                    }
                }

                break;
            }
        }

        if (property == default) {
            v = default;
            return false;
        }

        if (TryUnwindProperty(t, property, out var genericValue)) {
            v = genericValue is not T value ? default : value;
#pragma warning disable CA1508
            return v != null;
#pragma warning restore CA1508
        }

        v = default;
        return false;
    }

    public bool TryGetProperty<T>(string name, [MaybeNullWhen(false)] out T o) => TryGetProperty(name, 0, out o);

    public bool TryGetProperty<T>(string name, int instance, [MaybeNullWhen(false)] out T o) => TryGetProperty(new FName(name, instance), out o);

    private static bool TryUnwindProperty(Type? t, UProperty? property, out object? v) {
        if (t == null || property == null) {
            v = default;
            return false;
        }

        if (t.IsInstanceOfType(property)) {
            v = property;
            return true;
        }

        var propertyValue = property.GetValue();
        if (t.IsAssignableFrom(propertyValue?.GetType())) {
            v = propertyValue;
            return true;
        }

        switch (propertyValue) {
            case List<UProperty?> list: {
                var arr = (Array) Activator.CreateInstance(t, list.Count)!;
                for (var index = 0; index < list.Count; index++) {
                    var entry = list[index];
                    if (entry != null && TryUnwindProperty(t.GetElementType()!, entry, out var entryValue)) {
                        arr.SetValue(entryValue, index);
                    }
                }

                v = arr;
                return true;
            }
            case List<KeyValuePair<UProperty?, UProperty?>> dictionary: // maps are lists, because i don't trust something funky happening with keys
            {
                var dict = (IDictionary) Activator.CreateInstance(t, dictionary.Count)!;
                var keyType = t.GetGenericArguments()[0];
                var valueType = t.GetGenericArguments()[1];
                foreach (var entry in dictionary) {
                    if (entry.Key == null || !TryUnwindProperty(keyType, entry.Key, out var key) || key == null) {
                        continue;
                    }

                    if (entry.Value == null || !TryUnwindProperty(valueType, entry.Value, out var value)) {
                        value = valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
                    }

                    dict.Add(key, value);
                }

                v = dict;
                return true;
            }
            default:
                v = null;
                return false;
        }
    }
}
