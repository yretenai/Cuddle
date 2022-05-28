using System;
using System.Collections.Generic;
using System.Linq;
using Cuddle.Core.Structs;
using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Core.Assets;

public class UObject {
    public UObject(FArchiveReader data) {
        Owner = data.Asset!;

        while (data.Remaining > 0) {
            var tag = new FPropertyTag(data);
            if (tag.Name == "None") {
                break;
            }

            var start = data.Position;
            var expectedEnd = start + tag.Size;

            try {
                Properties[tag] = FProperty.Create(data, tag);
            } catch (Exception e) {
                Log.Error(e, "Error while deserializing {Type} for property {Name} in {ObjectName}", tag.Type, tag.Name, GetType().Name);
                data.Position = expectedEnd;
                continue;
            }

            if (data.Position != expectedEnd) {
                Log.Warning("Did not deserialize {Type} for property {Name} in {ObjectName} correctly!", tag.Type, tag.Name, GetType().Name);
            }

            data.Position = expectedEnd;
        }
    }

    public UAssetFile Owner { get; }

    public Dictionary<FPropertyTag, FProperty?> Properties { get; } = new();
    public FProperty? this[FName key] => Properties.FirstOrDefault(x => x.Key.Name == key).Value;
    public FProperty? this[FName key, int index] => Properties.FirstOrDefault(x => x.Key.Name == key && x.Key.Index == index).Value;
}
