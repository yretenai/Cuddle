using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Cuddle.Core.Enums;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Core.Assets;

public record UObject : PropertyOwner {
    // ReSharper disable once MemberCanBeProtected.Global
    public UObject(FArchiveReader data, FObjectExport export) {
        Owner = data.Asset!;
        Export = export;
        Properties = ReadProperties(data, new FPropertyTagContext { ElementTag = new FPropertyTag { Name = new FName(GetType().Name) } }, GetType().Name);
        if (!Export.ObjectFlags.HasFlag(EObjectFlags.ClassDefaultObject) && data.ReadBoolean()) {
            Guid = data.Read<Guid>();
        }
    }

    [JsonIgnore]
    public FObjectExport Export { get; }

    [JsonIgnore]
    public Guid Guid { get; }

    [JsonIgnore]
    public UAssetFile Owner { get; }

    public UProperty? this[string key] => Properties.FirstOrDefault(x => x.Key.Name.Value == key).Value;
    public UProperty? this[string key, int index] => Properties.FirstOrDefault(x => x.Key.Name.Value == key && x.Key.Index == index).Value;
    public UProperty? this[string key, int index, int instance] => Properties.FirstOrDefault(x => x.Key.Name.Value == key && x.Key.Name.Instance == instance && x.Key.Index == index).Value;
    public UProperty? this[FName key] => Properties.FirstOrDefault(x => x.Key.Name == key).Value;
    public UProperty? this[FName key, int index] => Properties.FirstOrDefault(x => x.Key.Name == key && x.Key.Index == index).Value;
    internal virtual bool SerializeProperties => true;

    public static Dictionary<FPropertyTag, UProperty?> ReadProperties(FArchiveReader data, FPropertyTagContext context, string name) {
        var properties = new Dictionary<FPropertyTag, UProperty?>();
        while (data.Remaining > 0) {
            var tag = new FPropertyTag(data, context);
            if (tag.Name == "None") {
                break;
            }

            var start = data.Position;
            var expectedEnd = start + tag.Size;
            var errored = false;

            try {
                properties[tag] = UProperty.CreateProperty(data, tag, context);
            } catch (Exception e) {
                Log.Error(e, "Error while deserializing {Type} for property {Name} ({Context}) in {ObjectName}", tag.Type, tag, context, name);
                errored = true;
            } finally {
                if (context.ReadMode == FPropertyReadMode.Normal) {
                    if (data.Position != expectedEnd && !errored) {
                        Log.Warning("Did not deserialize {Type} for property {Name} ({Context}) in {ObjectName} correctly!", tag.Type, tag, context, name);
                    }

                    data.Position = expectedEnd;
                }
            }
        }

        return properties;
    }
}
