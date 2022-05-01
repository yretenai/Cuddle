using Cuddle.Core.FileSystem;

namespace Cuddle.Core.Structs.Asset;

public class FNameEntry {
    public FNameEntry() => Name = "None";

    public FNameEntry(FArchive archive) {
        Name = archive.ReadString();
        NonCasePreservingHash = archive.Read<ushort>();
        CasePreservingHash = archive.Read<ushort>();
    }

    public string Name { get; set; }
    public ushort NonCasePreservingHash { get; set; }
    public ushort CasePreservingHash { get; set; }

    public override string ToString() => Name;

    public override int GetHashCode() => CasePreservingHash;
}
