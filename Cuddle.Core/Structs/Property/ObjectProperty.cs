using Cuddle.Core.Assets;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class ObjectProperty : UProperty {
    public ObjectProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(tag, context, data.Asset) => Value = new FPackageIndex(data);

    public FPackageIndex Value { get; }
    public override object GetValue() => Value;
}
