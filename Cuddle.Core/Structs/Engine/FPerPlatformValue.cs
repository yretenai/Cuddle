using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core.Structs.Engine;

[ObjectRegistration(Skip = true)]
public abstract record FPerPlatformValue<T> : FTaggedStructValue where T : unmanaged {
    protected FPerPlatformValue() : this(default(T)) { }

    protected FPerPlatformValue(FPropertyOwner owner) => Owner = owner;

    protected FPerPlatformValue(T @default) => Default = @default;

    public T Default { get; set; }
    internal override bool SerializeProperties => false;

    public override void ProcessProperties(FPropertyOwner owner, EObjectVersion version, EObjectVersionUE5 versionUE5) {
        Default = owner.GetProperty<T>(nameof(Default));
    }

    public void Deconstruct(out T @default) {
        @default = Default;
    }
}
