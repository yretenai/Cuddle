namespace Cuddle.Core.Structs.Asset;

public abstract class FObjectAbstract {
    public FName ObjectName { get; protected init; } = FName.Null;

    public override string ToString() => ObjectName;
}
