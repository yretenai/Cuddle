using Cuddle.Core.Assets;
using Cuddle.Core.Objects;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core.Structs.Engine;

[ObjectRegistration(Skip = true)]
public abstract record FPerPlatformValue<T> : FTaggedStructValue where T : unmanaged {
    protected FPerPlatformValue() : this(default(T)) { }

    protected FPerPlatformValue(PropertyOwner owner) {
        Owner = owner;
    }

    protected FPerPlatformValue(T @default) {
        Default = @default;
    }

    public override void ProcessProperties(PropertyOwner owner)
    {
        Default = owner.GetProperty<T>(nameof(Default));
    }

    public T Default { get; set; }
    internal override bool SerializeProperties => false;

    public void Deconstruct(out T @default) {
        @default = Default;
    }
}

public record FPerPlatformFloat : FPerPlatformValue<float> {
    public FPerPlatformFloat(float @default) : base(@default) { }
}

public record FPerPlatformInt : FPerPlatformValue<int>{
    public FPerPlatformInt(int @default) : base(@default) { }
}

public record FPerPlatformBool : FPerPlatformValue<bool>{
    public FPerPlatformBool(bool @default) : base(@default) { }
}
