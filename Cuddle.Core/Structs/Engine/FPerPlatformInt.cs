namespace Cuddle.Core.Structs.Engine;

public record FPerPlatformInt : FPerPlatformValue<int> {
    public FPerPlatformInt(int @default) : base(@default) { }
}
