﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Cuddle.Core.Objects;
using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Core.Assets;

public static class FStructRegistry {
    static FStructRegistry() {
        LoadTypes(typeof(FStructValue).Assembly);
    }

    private static Dictionary<string, Type> StructTypes { get; } = new() {
        { "Guid", typeof(Guid) },
        { "Vector", typeof(Vector3) },
        { "Vector2D", typeof(Vector2) },
        { "Quat", typeof(Quaternion) },
    };

    private static Dictionary<Regex, Type> RegexStructTypes { get; } = new();

    public static void LoadTypes(Assembly assembly) {
        foreach (var type in assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(FStructValue)))) {
            if (type == typeof(FStructValue)) {
                continue;
            }

            var attr = type.GetCustomAttribute<ObjectRegistrationAttribute>();
            if (attr != null) {
                if (attr.Skip) {
                    continue;
                }

                if (!string.IsNullOrEmpty(attr.Expression)) {
                    RegexStructTypes[new Regex(attr.Expression, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)] = type;
                }
            }

            if (attr?.Names?.Length > 0) {
                foreach (var name in attr.Names) {
                    StructTypes[name] = type;
                }
            }

            {
                var name = type.Name;
                if (name[0] is 'F' or 'U') {
                    name = name[1..];
                }

                StructTypes[name] = type;
            }
        }
    }

    public static object? Create(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) {
        var className = tag.ValueType.Value;

        if (!StructTypes.TryGetValue(className, out var structType)) {
            foreach (var (regex, type) in RegexStructTypes) {
                if (regex.IsMatch(className)) {
                    // cache this class name because the regex matched so we can look it up faster next time.
                    StructTypes[className] = type;
                    structType = type;
                    break;
                }
            }
        }

        if (structType == null) {
            try {
                return new FTaggedStructValue(data, new FPropertyTagContext(tag, null, FPropertyReadMode.Normal, context.IsGVAS), tag.Name);
            } catch (Exception e) {
                Log.Error(e, "Error while deserializing {Type} for property {Name} for context {Context}", className, tag, context);
                throw;
            }
        }

        try {
            return structType.IsValueType ? data.Read(structType) : Activator.CreateInstance(structType, data);
        } catch {
            return null;
        }
    }
}