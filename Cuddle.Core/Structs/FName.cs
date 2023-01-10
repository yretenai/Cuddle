using System;
using System.Collections.Generic;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs;

public readonly record struct FName : IEquatable<string?> {
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
    }

    public FName(string value, int instance = 0) {
        var index = value.IndexOf(':', StringComparison.Ordinal);
        if (index > -1 && int.TryParse(value[(index + 1)..], out instance)) {
            value = value[..index];
        }

        Index = -1;
        Value = value;
        Instance = instance;
    }

    public static FName Null { get; } = new() { Index = -1 };

    public int Index { get; private init; }

    // Deduplication instance. 1 = first entry, 0 = no duplicates allowed.
    public int Instance { get; init; }
    public string Value { get; } = "None";
    public string InstanceValue => Instance > 1 ? $"{Value}:{Instance}" : Value;

    public bool Equals(FName other) => EqualityComparer<int>.Default.Equals(Instance, other.Instance) && EqualityComparer<string>.Default.Equals(Value, other.Value);
    public bool Equals(string? other) => Instance < 2 && EqualityComparer<string>.Default.Equals(Value, other);

    public static implicit operator string(FName? name) => name?.Value ?? "None";

    public override string ToString() => Value;
    public bool Equals(string? other, int instance) => EqualityComparer<int>.Default.Equals(Instance, instance) && EqualityComparer<string>.Default.Equals(Value, other);
    public override int GetHashCode() => HashCode.Combine(Value, Instance);
}
