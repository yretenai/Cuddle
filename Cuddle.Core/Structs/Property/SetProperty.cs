using System.Collections.Generic;
using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class SetProperty : UProperty {
    public SetProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(data, tag, context) {
        var arrayContext = context with { ElementTag = tag, ReadMode = FPropertyReadMode.Array };
        var deserializeTag = tag.AsValueTag();
        if (deserializeTag.Type.Value is "StructProperty" or "ArrayProperty") {
            deserializeTag = new FPropertyTag(data, context);
            arrayContext = arrayContext with { ContextTag = deserializeTag };
        }

        var count = data.Read<int>();
        for (var i = 0; i < count; ++i) {
            Value.Add(CreateProperty(data, deserializeTag, arrayContext));
        }

        count = data.Read<int>();
        for (var i = 0; i < count; ++i) {
            Value.Add(CreateProperty(data, deserializeTag, arrayContext));
        }
    }

    public List<UProperty?> Value { get; } = new();
    public override object GetValue() => Value;
}
