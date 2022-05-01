using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Cuddle.Core.Enums;

namespace Cuddle.Core.FileSystem;

public class FArchive {
    public FArchive(UAssetFile asset, ReadOnlyMemory<byte> data) {
        Asset = asset;
        Data = data;
        // todo?
    }

    public EGame Game => Asset.Game;
    public EObjectVersion Version => Asset.Summary.FileVersionUE4;
    public UAssetFile Asset { get; }
    public ReadOnlyMemory<byte> Data { get; }

    public int Position { get; set; }

    public T Read<T>() where T : struct {
        var value = MemoryMarshal.Read<T>(Data.Span[Position..]);
        Position += Unsafe.SizeOf<T>();
        return value;
    }

    public bool ReadBoolean() {
        var value = Read<uint>();
        if (value > 1) {
            throw new InvalidDataException($"Expected 0 or 1 for a boolean value, got {value}");
        }

        return value == 1;
    }

    public ReadOnlyMemory<T> ReadArray<T>(int? count = null) where T : struct {
        count ??= Read<int>();

        var value = MemoryMarshal.Cast<byte, T>(Data.Span[Position..])[..count.Value];
        Position += Unsafe.SizeOf<T>() * count.Value;
        return value.ToArray().AsMemory();
    }

    public T[] ReadClassArray<T>(int? count = null) where T : class, new() {
        count ??= Read<int>();

        var value = new T[count.Value];
        var type = typeof(T);
        for (var i = 0; i < value.Length; ++i) {
            value[i] = (T) Activator.CreateInstance(type, this)!;
        }

        Position += Unsafe.SizeOf<T>() * count.Value;
        return value;
    }

    public string ReadString(int? count = null) {
        count ??= Read<int>();
        var value = "None";

        switch (count) {
            case > 1:
                value = Encoding.UTF8.GetString(Data.Span.Slice(Position, count.Value - 1));
                Position += count.Value;
                break;
            case < -1:
                value = Encoding.Unicode.GetString(Data.Span.Slice(Position, (0 - count.Value - 1) * 2));
                Position += (0 - count.Value) * 2;
                break;
        }

        return value;
    }

    public string[] ReadStrings(int? count = null) {
        count ??= Read<int>();

        var value = new string[count.Value];
        for (var i = 0; i < value.Length; ++i) {
            value[i] = ReadString();
        }

        return value;
    }
}
