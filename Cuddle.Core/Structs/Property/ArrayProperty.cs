using Cuddle.Core.Assets;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class ArrayProperty : UProperty {
    public ArrayProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(tag, context) {
        var count = data.Read<int>();

        var arrayContext = context with { ElementTag = tag, ReadMode = FPropertyReadMode.Array };
        var deserializeTag = tag.AsValueTag();
        if (deserializeTag.Type.Value is "StructProperty" && data.Version >= EObjectVersion.INNER_ARRAY_TAG_INFO) {
            deserializeTag = new FPropertyTag(data, context);
            arrayContext = arrayContext with { ContextTag = deserializeTag };
        }

        Value = new UProperty?[count];
        for (var i = 0; i < count; ++i) {
            Value[i] = CreateProperty(data, deserializeTag, arrayContext);
        }
    }

    public UProperty?[] Value { get; }
    public override object GetValue() => Value;
}
