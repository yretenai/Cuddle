using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class UnmanagedProperty<T> : UProperty where T : unmanaged {
    public UnmanagedProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(data, tag, context) => Value = data.Read<T>();

    public T Value { get; }

    public override string ToString() => Value.ToString()!;
    public override object GetValue() => Value;
}
