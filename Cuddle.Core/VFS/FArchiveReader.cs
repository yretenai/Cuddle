﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Cuddle.Core.Assets;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.Asset;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core.VFS;

public sealed class FArchiveReader : IPoliteDisposable {
    public FArchiveReader(UAssetFile asset, MemoryOwner<byte> data) {
        Asset = asset;
        Data = data;
        Game = asset.Game;
        Version = asset.Summary.FileVersionUE4;
        VersionUE5 = asset.Summary.FileVersionUE5;
    }

    public FArchiveReader(EGame game, MemoryOwner<byte> data, VFSManager? manager = null) {
        Game = game;
        Version = game.FindObjectVersion();
        Manager = manager;
        Data = data;
    }

    public FArchiveReader(MemoryOwner<byte> data, VFSManager? manager = null) {
        Data = data;
        Manager = manager;
    }

    public VFSManager? Manager { get; set; }
    public EGame Game { get; set; }
    public EObjectVersion Version { get; set; }
    public EObjectVersionUE5 VersionUE5 { get; set; }
    public UAssetFile? Asset { get; internal set; }
    public MemoryOwner<byte> Data { get; }
    public int Position { get; set; }
    public int Length => Data.Length;
    public int Remaining => Data.Length - Position;
    public bool HasEditorData => Asset?.Summary.HasEditorData == true;

    public bool Disposed { get; private set; }

    public void Dispose() {
        Data.Dispose();
        if (Disposed) {
            return;
        }

        GC.SuppressFinalize(this);
        Disposed = true;
    }

    ~FArchiveReader() {
        Dispose();
    }

    public T Read<T>() where T : unmanaged {
        var value = MemoryMarshal.Read<T>(Data.Span[Position..]);
        Position += Unsafe.SizeOf<T>();
        return value;
    }

    public unsafe object Read(Type t) {
        if (!t.IsValueType) {
            throw new NotSupportedException();
        }

        var size = Marshal.SizeOf(t);
        using var data = Data.Memory.Slice(Position, size).Pin();
        Position += size;
        return Marshal.PtrToStructure((IntPtr) data.Pointer, t)!;
    }

    public bool ReadBoolean() {
        var value = Read<uint>();
        if (value > 1) {
            throw new InvalidDataException($"Expected 0 or 1 for a boolean value, got {value}");
        }

        return value == 1;
    }

    public bool ReadBit() {
        var value = Read<byte>();
        if (value > 1) {
            throw new InvalidDataException($"Expected 0 or 1 for a bit value, got {value}");
        }

        return value == 1;
    }

    public Span<T> ReadArray<T>(int? count = null) where T : unmanaged {
        count ??= Read<int>();

        var value = new T[count.Value].AsSpan();
        var size = Unsafe.SizeOf<T>() * count.Value;
        Data.Span.Slice(Position, size).CopyTo(MemoryMarshal.AsBytes(value));
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
            case >= 1:
                value = Encoding.UTF8.GetString(Data.Span.Slice(Position, count.Value - 1));
                Position += count.Value;
                break;
            case <= -1:
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

    public FName[] ReadNames(int? count = null) {
        count ??= Read<int>();

        var value = new FName[count.Value];
        for (var index = 0; index < value.Length; ++index) {
            value[index] = new FName(this);
        }

        return value;
    }

    public FArchiveReader Partition(int pos, int size) {
        var block = MemoryOwner<byte>.Allocate(size);
        Data.Memory.Slice(pos, size).CopyTo(block.Memory);
        return Asset == null ? new FArchiveReader(Game, block, Manager) : new FArchiveReader(Asset, block);
    }

    public FArchiveReader Partition(int? count = null) {
        count ??= Read<int>();
        var block = MemoryOwner<byte>.Allocate(count.Value);
        Data.Memory.Slice(Position, count.Value).CopyTo(block.Memory);
        Position += count.Value;
        return Asset == null ? new FArchiveReader(Game, block, Manager) : new FArchiveReader(Asset, block);
    }
}
