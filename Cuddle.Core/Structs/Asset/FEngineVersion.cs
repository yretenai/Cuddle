using Cuddle.Core.FileSystem;

namespace Cuddle.Core.Structs.Asset;

public class FEngineVersion {
    public FEngineVersion() { }

    public FEngineVersion(FArchive archive) {
        Major = archive.Read<ushort>();
        Minor = archive.Read<ushort>();
        Patch = archive.Read<ushort>();
        Changeset = archive.Read<uint>();
        Branch = archive.ReadString();
    }

    public ushort Major { get; set; }
    public ushort Minor { get; set; }
    public ushort Patch { get; set; }
    public uint Changeset { get; set; }
    public string Branch { get; set; } = "None";
}
