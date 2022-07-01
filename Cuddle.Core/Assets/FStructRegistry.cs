using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.Structs.Math;
using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Core.Assets;

public static class FStructRegistry {
    static FStructRegistry() {
        LoadTypes(typeof(FFallbackStruct).Assembly);
    }

    private static List<(string Name, Type? T, EObjectVersion MaximumVersionUE4, EObjectVersionUE5 MaximumVersionUE5)> StructTypes { get; } = new() {
        ("Guid", typeof(Guid), (EObjectVersion) uint.MaxValue, (EObjectVersionUE5) uint.MaxValue),
        ("ColorMaterialInput", typeof(FMaterialInput<FColor>), (EObjectVersion) uint.MaxValue, (EObjectVersionUE5) uint.MaxValue),
        ("ScalarMaterialInput", typeof(FMaterialInput<float>), (EObjectVersion) uint.MaxValue, (EObjectVersionUE5) uint.MaxValue),
        ("ShadingModelMaterialInput", typeof(FMaterialInput<uint>), (EObjectVersion) uint.MaxValue, (EObjectVersionUE5) uint.MaxValue),
        ("StrataMaterialInput", typeof(FMaterialInput<uint>), (EObjectVersion) uint.MaxValue, (EObjectVersionUE5) uint.MaxValue),
        ("VectorMaterialInput", typeof(FMaterialInput<Vector3>), (EObjectVersion) uint.MaxValue, (EObjectVersionUE5) uint.MaxValue),
        ("Vector2MaterialInput", typeof(FMaterialInput<Vector2>), (EObjectVersion) uint.MaxValue, (EObjectVersionUE5) uint.MaxValue),
        ("MaterialAttributesInput", typeof(FExpressionInput), (EObjectVersion) uint.MaxValue, (EObjectVersionUE5) uint.MaxValue),
    };

    private static List<(Regex Test, Type? T, EObjectVersion MaximumVersionUE4, EObjectVersionUE5 MaximumVersionUE5)> RegexStructTypes { get; } = new();

    public static void LoadTypes(Assembly assembly) {
        foreach (var type in assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(FFallbackStruct)) || x.GetCustomAttributes<ObjectRegistrationAttribute>().Any() && x.IsValueType)) {
            if (type == typeof(FFallbackStruct)) {
                continue;
            }

            if (type.IsGenericType) {
                continue;
            }

            var attrs = type.GetCustomAttributes<ObjectRegistrationAttribute>().ToArray();
            foreach (var attr in attrs) {
                if (attr.Skip) {
                    continue;
                }

                if (!string.IsNullOrEmpty(attr.Expression)) {
                    RegexStructTypes.Add((new Regex(attr.Expression, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant), type, attr.MaxVersionUE4, attr.MaxVersionUE5));
                }

                if (attr.Names?.Length > 0) {
                    foreach (var name in attr.Names) {
                        StructTypes.Add((name, type, attr.MaxVersionUE4, attr.MaxVersionUE5));
                    }
                }
            }

            if (attrs.Length == 0) {
                var name = type.Name;
                if (name[0] is 'F' or 'U') {
                    name = name[1..];
                }

                StructTypes.Add((name, type, (EObjectVersion) uint.MaxValue, (EObjectVersionUE5) uint.MaxValue));
            }
        }
    }

    public static Type? GetType(string name, EObjectVersion version, EObjectVersionUE5 ue5Version) {
        var t = StructTypes.FirstOrDefault(x => x.Name == name && x.MaximumVersionUE5 > ue5Version && x.MaximumVersionUE4 > version).T;
        if (t != null) {
            return t;
        }

        t = RegexStructTypes.FirstOrDefault(x => x.Test.IsMatch(name) && x.MaximumVersionUE5 > ue5Version && x.MaximumVersionUE4 > version).T;
        if (t != null) {
            // cache for faster lookup.
            StructTypes.Add((name, t, version, ue5Version));
            return t;
        }

        return null;
    }

    public static object? Create(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) {
        var className = tag.ValueType.Value;

        var structType = GetType(className, data.Version, data.VersionUE5);

        if (structType == null) {
            try {
                return new FTaggedStructValue(data, new FPropertyTagContext(tag, null, FPropertyReadMode.Normal, context.IsGVAS), tag.Name);
            } catch (Exception e) {
                Log.Error(e, "Error while deserializing {Type} for property {Name} for context {Context}", className, tag, context);
                throw;
            }
        }

        if (structType.IsValueType) {
            return data.Read(structType);
        }

        try {
            var value = Activator.CreateInstance(structType, data) as FTaggedStructValue;
            value?.ProcessProperties(value);
            return value;
        } catch {
            return null;
        }
    }
}
