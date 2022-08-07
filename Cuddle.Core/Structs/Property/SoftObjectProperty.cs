using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class SoftObjectProperty : UProperty {
    public SoftObjectProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(tag, context, data.Asset) => Value = new FSoftObjectPath(data);

    public FSoftObjectPath Value { get; }

    public override string ToString() => Value.Path + "/" + Value.SubPath;
    public override object GetValue() => Value;
}
