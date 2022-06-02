using System.Collections.Generic;
using System.Text.Json.Serialization;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Assets;

[ObjectRegistration(Skip = true)]
public record FTaggedStructValue : FStructValue {
    public FTaggedStructValue() { }

    public FTaggedStructValue(FArchiveReader data, FPropertyTagContext context, FName name) => Properties = UObject.ReadProperties(data, context, name);

    [JsonIgnore]
    public Dictionary<FPropertyTag, UProperty?> Properties { get; } = new();

    internal virtual bool SerializeProperties => true;
}
