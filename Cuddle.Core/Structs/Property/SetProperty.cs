using System.Collections.Generic;
using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class SetProperty : UProperty {
    public SetProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(data, tag, context) {
        var arrayContext = context with { ElementTag = tag, ReadMode = FPropertyReadMode.Array };

        var count = data.Read<int>();
        var elementTag = tag.AsValueTag();
        for (var i = 0; i < count; ++i) {
            Value.Add(CreateProperty(data, elementTag, arrayContext));
        }

        count = data.Read<int>();
        for (var i = 0; i < count; ++i) {
            Value.Add(CreateProperty(data, elementTag, arrayContext));
        }
    }

    public List<UProperty?> Value { get; } = new();
}
