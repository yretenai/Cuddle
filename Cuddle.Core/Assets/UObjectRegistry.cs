using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core.Assets;

public static class UObjectRegistry {
    static UObjectRegistry() {
        LoadTypes(typeof(UObject).Assembly);
    }

    private static Dictionary<string, Type> ObjectTypes { get; } = new();
    private static Dictionary<Regex, Type> RegexObjectTypes { get; } = new();

    public static void LoadTypes(Assembly assembly) {
        foreach (var type in assembly.GetTypes().Where(x => x.IsAssignableTo(typeof(UObject)))) {
            if (type == typeof(UObject)) {
                continue;
            }

            var attr = type.GetCustomAttribute<ObjectRegistrationAttribute>();
            if (attr != null) {
                if (attr.Skip) {
                    continue;
                }

                if (!string.IsNullOrEmpty(attr.Expression)) {
                    RegexObjectTypes[new Regex(attr.Expression, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)] = type;
                }
            }

            if (attr?.Names?.Length > 0) {
                foreach (var name in attr.Names) {
                    ObjectTypes[name] = type;
                }
            }

            {
                var name = type.Name;
                if (name[0] is 'F' or 'U') {
                    name = name[1..];
                }

                ObjectTypes[name] = type;
            }
        }
    }

    public static UObject? Create(string? className, FObjectExport export, UAssetFile uasset) {
        using var data = uasset.ExportData.Partition((int) export.SerialOffset, (int) export.SerialSize);

        if (string.IsNullOrEmpty(className)) {
            className = "Object";
        }

        var objectType = typeof(UObject);
        if (className != "Object" && !ObjectTypes.TryGetValue(className, out objectType)) {
            foreach (var (regex, type) in RegexObjectTypes) {
                if (regex.IsMatch(className)) {
                    // cache this class name because the regex matched so we can look it up faster next time.
                    ObjectTypes[className] = type;
                    objectType = type;
                    break;
                }
            }
        }

        try {
            return Activator.CreateInstance(objectType ?? typeof(UObject), data, export) as UObject;
        } catch {
            return null;
        }
    }
}
