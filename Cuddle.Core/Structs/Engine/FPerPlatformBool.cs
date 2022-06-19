namespace Cuddle.Core.Structs.Engine;

public record FPerPlatformBool : FPerPlatformValue<bool> {
    public FPerPlatformBool(bool @default) : base(@default) { }
}
