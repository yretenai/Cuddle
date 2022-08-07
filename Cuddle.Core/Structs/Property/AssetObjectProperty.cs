using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class AssetObjectProperty : UProperty {
    public AssetObjectProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(tag, context, data.Asset) => Value = data.ReadString();

    public string Value { get; }
    public override string ToString() => Value;
    public override object GetValue() => Value;
}
