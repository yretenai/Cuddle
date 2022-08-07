using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class StructProperty : UProperty {
    public StructProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(tag, context, data.Asset) => Value = FStructRegistry.Create(data, tag, context);

    public object? Value { get; }
    public override object? GetValue() => Value;
}
