using System.Collections.Generic;
using Cuddle.Core.Assets;
using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Core.Structs.Property;

public class MapProperty : UProperty {
    public MapProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(data, tag, context) {
        var keyTag = tag.AsKeyTag();
        var valueTag = tag.AsValueTag();
        
        // Unreal gets this data from the UField constructor, we don't have this data per-se.
        // todo: investigate if we can deserialize UField/UStruct/UClass.
        if (tag.KeyType.Value == "StructProperty") {
            if (context.ElementTag.Name.Value is not "None") {
                valueTag = valueTag with {
                    ValueType = new FName(context.ElementTag.Name.Value + "." + tag.Name.Value switch {
                        // todo: add struct types here such as: ParentType.FieldName, I.E Context.StructMap
                        _ => "None",
                    }),
                };
            }

            if (valueTag.ValueType.Value is "None") {
                Log.Error("Cannot deserialize Map<struct, ?> for {Context}", context);
            }

            return;
        }

        var arrayContext = context with { ElementTag = tag, ReadMode = FPropertyReadMode.Map };

        var count = data.Read<int>();
        for (var i = 0; i < count; ++i) {
            var key = CreateProperty(data, keyTag, arrayContext with { ContextTag = keyTag });
            Value.Add(new KeyValuePair<UProperty?, UProperty?>(key, null));
        }

        count = data.Read<int>();
        for (var i = 0; i < count; ++i) {
            var key = CreateProperty(data, keyTag, arrayContext with { ContextTag = keyTag });
            var value = CreateProperty(data, valueTag, arrayContext with { ContextTag = valueTag });
            Value.Add(new KeyValuePair<UProperty?, UProperty?>(key, value));
        }
    }

    public List<KeyValuePair<UProperty?, UProperty?>> Value { get; } = new();
    public override object GetValue() => Value;
}
