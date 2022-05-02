namespace Cuddle.Core.Structs.Asset;

public class FNameEntry {
    public FNameEntry() => Name = "None";

    public FNameEntry(FArchive archive) {
        Name = archive.ReadString();
        if (Name.Length == 0) {
            Name = "None";
        }

        NonCasePreservingHash = archive.Read<ushort>();
        CasePreservingHash = archive.Read<ushort>();
    }

    public string Name { get; }
    public ushort NonCasePreservingHash { get; }
    public ushort CasePreservingHash { get; }

    public override string ToString() => Name;

    public override int GetHashCode() => CasePreservingHash;
}
