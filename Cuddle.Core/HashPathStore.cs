using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Cuddle.Core.VFS;
using DragonLib.Hash;
using DragonLib.Hash.Basis;

namespace Cuddle.Core;

public class HashPathStore : IArchiveSerializable {
    public HashPathStore(Encoding? encoding = null) => Encoding = encoding ?? Encoding.Unicode;

    public HashPathStore(FArchiveReader archive, Encoding? encoding = null) {
        Encoding = encoding ?? Encoding.Unicode;
        var count = archive.Read<int>();
        Paths.EnsureCapacity(count);
        for (var index = 0; index < count; ++index) {
            Paths[archive.Read<ulong>()] = archive.ReadString();
        }
    }

    public Encoding Encoding { get; } // Some platforms use UTF8, most will use UTF16 though.
    public Dictionary<ulong, string> Paths { get; } = new();

    public void Serialize(FArchiveWriter writer) {
        writer.Write(Paths.Count);

        foreach (var (hash, path) in Paths) {
            writer.Write(hash);
            writer.Write(path);
        }
    }

    public ulong AddPath(string relativePath, ulong seed, bool bugged) {
        var lowercaseRelativePath = relativePath.ToLower();
        var basis = 0xcbf29ce484222325UL;
        var prime = 0x00000100000001b3UL;
        if (bugged) { // they swapped prime and basis for a while
            (basis, prime) = (prime, basis);
        }

        basis += seed;

        using var fnv = FowlerNollVo.CreateAlternate((FNV64Basis) basis, prime);
        var hash = fnv.ComputeHashValue(Encoding.GetBytes(lowercaseRelativePath));
        Paths[hash] = relativePath;
        return hash;
    }

    public bool TryGetPath(ulong value, [MaybeNullWhen(false)] out string path) {
        path = null;

        if (Paths.TryGetValue(value, out var text)) {
            path = text;
            return true;
        }

        return false;
    }

    public ReadOnlyMemory<byte> Serialize() {
        var writer = new FArchiveWriter();
        Serialize(writer);
        return writer.Data;
    }
}
