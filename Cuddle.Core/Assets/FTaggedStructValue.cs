using System.Text.Json.Serialization;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Assets;

[ObjectRegistration(Skip = true)]
public record FTaggedStructValue : PropertyOwner, FStructValue {
    protected FTaggedStructValue() { }

    public FTaggedStructValue(FArchiveReader data, FPropertyTagContext context, FName name) => Properties = UObject.ReadProperties(data, context, name);

    public virtual void ProcessProperties(PropertyOwner owner) { }

    [JsonIgnore]
    public PropertyOwner? Owner { get; init; }

    internal virtual bool SerializeProperties => Owner == null;
}
