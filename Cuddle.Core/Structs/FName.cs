using System;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs;

public readonly record struct FName {
    public FName() {
        Index = -1;
        Instance = 0;
    }

    public FName(FArchiveReader archive) {
        Index = archive.Read<int>();
        Instance = archive.Read<int>();

        if (archive.Asset == null) {
            return;
        }

        if (Index >= archive.Asset.Names.Length || Index < 0) {
            throw new IndexOutOfRangeException($"FName index {Index} is out of range!");
        }

        Value = archive.Asset.Names[Index].Name;
        if (Instance > 0) {
            Value += $".{Instance}";
        }
    }

    public FName(string value, int instance = 0) {
        Value = value;
        Instance = instance;
        if (Instance > 0) {
            Value += $".{Instance}";
        }
    }

    public static FName Null { get; } = new() { Index = -1 };

    public int Index { get; private init; }
    public int Instance { get; }
    public string Value { get; } = "None";

    public static implicit operator string(FName? name) => name?.Value ?? "None";

    public override string ToString() => Value;
    public override int GetHashCode() => Value.GetHashCode();
}
