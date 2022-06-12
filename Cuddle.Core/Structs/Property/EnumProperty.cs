using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class EnumProperty : UProperty {
    public EnumProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(tag, context) => Value = new FName(data);

    public FName Value { get; }

    public override string ToString() => Value;
    public override object GetValue() => Value;
}
