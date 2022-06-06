using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class BoolProperty : UProperty {
    public BoolProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(data, tag, context) => Value = context.ReadMode == FPropertyReadMode.Normal ? tag.BoolValue : data.ReadBit();

    public bool Value { get; }

    public override string ToString() => Value.ToString();
    public override object GetValue() => Value;
}
