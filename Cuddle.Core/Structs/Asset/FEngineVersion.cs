namespace Cuddle.Core.Structs.Asset;

public struct FEngineVersion {
    public ushort Major { get; set; }
    public ushort Minor { get; set; }
    public ushort Patch { get; set; }
    public uint Changeset { get; set; }
}
