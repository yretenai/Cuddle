using System.Collections.Generic;
using Cuddle.Core.Assets;
using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Core.Structs.Property;

public class MapProperty : UProperty {
    public MapProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(data, tag, context) {
        if (tag.KeyType.Value == "StructType") {
            Log.Error("Cannot deserialize Map<struct, ?> for {Context}", context);
            return;
        }

        var arrayContext = context with { ElementTag = tag, ReadMode = FPropertyReadMode.Map };

        var count = data.Read<int>();
        var keyTag = tag.AsKeyTag();
        for (var i = 0; i < count; ++i) {
            var key = CreateProperty(data, keyTag, arrayContext);
            Value.Add(new KeyValuePair<UProperty?, UProperty?>(key, null));
        }

        count = data.Read<int>();
        var valueTag = tag.AsValueTag();
        for (var i = 0; i < count; ++i) {
            var key = CreateProperty(data, keyTag, arrayContext);
            var value = CreateProperty(data, valueTag, arrayContext);
            Value.Add(new KeyValuePair<UProperty?, UProperty?>(key, value));
        }
    }

    public List<KeyValuePair<UProperty?, UProperty?>> Value { get; } = new();
}
