using Cuddle.Core.Assets;
using Cuddle.Core.Structs.Intl;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class TextProperty : UProperty {
    public TextProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(tag, context) => Value = new FText(data);

    public FText Value { get; }

    public override string ToString() => Value.ToString()!;
    public override object GetValue() => Value;
}
