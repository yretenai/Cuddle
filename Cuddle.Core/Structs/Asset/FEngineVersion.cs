using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Asset;

public class FEngineVersion {
    public FEngineVersion() { }

    public FEngineVersion(FArchiveReader archive) {
        Major = archive.Read<ushort>();
        Minor = archive.Read<ushort>();
        Patch = archive.Read<ushort>();
        Changeset = archive.Read<uint>();
        Branch = archive.ReadString();
    }

    public ushort Major { get; init; }
    public ushort Minor { get; init; }
    public ushort Patch { get; init; }
    public uint Changeset { get; init; }
    public string Branch { get; init; } = "";
}
