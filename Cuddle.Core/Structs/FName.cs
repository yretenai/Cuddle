using System;

namespace Cuddle.Core.Structs;

public class FName {
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

        if (Index > archive.Asset.Names.Length) {
            throw new IndexOutOfRangeException($"FName index {Index} is out of range!");
        }

        Value = archive.Asset.Names[Index].Name;
        if (Instance > 0) {
            Value += $".{Value}";
        }
    }

    public static FName Null { get; } = new() { Index = -1 };

    public int Index { get; private init; }
    public int Instance { get; }
    public string Value { get; } = "None";

    public static implicit operator string(FName? name) => name?.Value ?? "None";

    public override string ToString() => Value;
}
