using Cuddle.Core.Structs.Property;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Assets;

public abstract class UProperty {
    protected UProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) {
        Tag = tag;
        TagContext = context;
    }

    public FPropertyTag Tag { get; }
    public FPropertyTagContext TagContext { get; }

    public static UProperty? Create(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) {
        return tag.Type.Value switch {
            "BoolProperty" => new BoolProperty(data, tag, context),
            "ByteProperty" when tag.ValueType.Value != "None" => new EnumProperty(data, tag, context),
            "ByteProperty" => new ByteProperty(data, tag, context),
            "StructProperty" => new StructProperty(data, tag, context),
            "ObjectProperty" => new ObjectProperty(data, tag, context),
            _ => null,
        };
    }
}
