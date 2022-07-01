using System.Text.Json.Serialization;
using Cuddle.Core.Structs.Property;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Assets;

public abstract class UProperty {
    protected UProperty(FPropertyTag tag, FPropertyTagContext context) {
        Tag = tag;
        TagContext = context;
    }

    [JsonIgnore]
    public FPropertyTag Tag { get; }

    [JsonIgnore]
    public FPropertyTagContext TagContext { get; }

    public static UProperty? CreateProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) {
        return tag.Type.Value switch {
            // todo: DelegateProperty
            // todo: FieldPathProperty
            // todo: InterfaceProperty
            // todo: LazyObjectProperty
            // todo: MulticastDelegateProperty
            // todo: MulticastInlineDelegateProperty
            // todo: MulticastSparseDelegateProperty
            "ArrayProperty" => new ArrayProperty(data, tag, context),
            "AssetClassProperty" => new AssetObjectProperty(data, tag, context),
            "AssetObjectProperty" => new AssetObjectProperty(data, tag, context),
            "BoolProperty" => new BoolProperty(data, tag, context),
            "ByteProperty" when tag.ValueType.Value != "None" => new EnumProperty(data, tag, context),
            "ByteProperty" => new ByteProperty(data, tag, context),
            "ClassProperty" => new ObjectProperty(data, tag, context),
            "DoubleProperty" => new UnmanagedProperty<double>(data, tag, context),
            "LargeWorldCoordinatesRealProperty" => new UnmanagedProperty<double>(data, tag, context), // I assume one day this will be a decimal aka 128-bit float.
            "EnumProperty" => new EnumProperty(data, tag, context),
            "FloatProperty" => new UnmanagedProperty<float>(data, tag, context),
            "Int8Property" => new UnmanagedProperty<sbyte>(data, tag, context),
            "Int16Property" => new UnmanagedProperty<short>(data, tag, context),
            "IntProperty" => new UnmanagedProperty<int>(data, tag, context),
            "Int64Property" => new UnmanagedProperty<long>(data, tag, context),
            "MapProperty" => new MapProperty(data, tag, context),
            "NameProperty" => new NameProperty(data, tag, context),
            "ObjectProperty" => new ObjectProperty(data, tag, context),
            "SetProperty" => new SetProperty(data, tag, context),
            "SoftClassProperty" => new SoftObjectProperty(data, tag, context),
            "SoftObjectProperty" => new SoftObjectProperty(data, tag, context),
            "StrProperty" => new StrProperty(data, tag, context),
            "StructProperty" => new StructProperty(data, tag, context),
            "TextProperty" => new TextProperty(data, tag, context),
            "UInt16Property" => new UnmanagedProperty<ushort>(data, tag, context),
            "UInt32Property" => new UnmanagedProperty<uint>(data, tag, context),
            "UInt64Property" => new UnmanagedProperty<ulong>(data, tag, context),
            "WeakObjectProperty" => new ObjectProperty(data, tag, context),
            _ => null,
        };
    }

    public abstract object? GetValue();
}
