using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class StrProperty : UProperty {
    public StrProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(tag, context) => Value = data.ReadString();

    public string Value { get; }

    public override string ToString() => Value;
    public override object GetValue() => Value;
}
