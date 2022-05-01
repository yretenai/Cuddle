using System;
using Cuddle.Core.FileSystem;

namespace Cuddle.Core.Structs;

public class FName {
    public FName() {
        Index = -1;
        Instance = 0;
    }

    public FName(FArchive archive) {
        Index = archive.Read<int>();
        if (Index > archive.Asset.Names.Length) {
            throw new IndexOutOfRangeException($"FName index {Index} is out of range!");
        }

        Instance = archive.Read<int>();

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
