using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Cuddle.Core.Enums;

namespace Cuddle.Core;

public class FArchive {
    public FArchive(UAssetFile? asset, ReadOnlyMemory<byte> data) {
        Asset = asset;
        Data = data;
    }

    public FArchive(EGame game, ReadOnlyMemory<byte> data) {
        Game = game;
        Version = game.ToGameObjectVersion();
        if (Version == 0) {
            Version = game.GetEngineVersion().ToObjectVersion();
        }

        Data = data;
    }

    public EGame Game { get; }
    public EObjectVersion Version { get; }
    public UAssetFile? Asset { get; }
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

    public Memory<T> ReadArray<T>(int? count = null) where T : struct {
        count ??= Read<int>();

        var value = new T[count.Value].AsMemory();
        var size = Unsafe.SizeOf<T>() * count.Value;
        Data.Span.Slice(Position, size).CopyTo(MemoryMarshal.AsBytes(value.Span));
        Position += size;
        return value;
    }

    public T[] ReadClassArray<T>(int? count = null, params object?[] extra) where T : class, new() {
        count ??= Read<int>();

        var value = new T[count.Value];
        var type = typeof(T);
        var args = new object?[extra.Length + 1];
        args[0] = this;
        extra.CopyTo(args, 1);
        for (var index = 0; index < value.Length; ++index) {
            value[index] = (T) Activator.CreateInstance(type, args)!;
        }

        Position += Unsafe.SizeOf<T>() * count.Value;
        return value;
    }

    public string ReadString(int? count = null) {
        count ??= Read<int>();
        var value = "";

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
        for (var index = 0; index < value.Length; ++index) {
            value[index] = ReadString();
        }

        return value;
    }
}
