using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using DragonLib.Hash;
using DragonLib.Hash.Basis;

namespace Cuddle.Core;

public class HashPathStore {
    public HashPathStore() { }

    public HashPathStore(FArchiveReader archive) {
        var count = archive.Read<int>();
        Paths.EnsureCapacity(count);
        for (var index = 0; index < count; ++index) {
            Paths[archive.Read<ulong>()] = archive.ReadString();
        }
    }

    public Dictionary<ulong, string> Paths { get; } = new();

    public void AddPath(string relativePath, string absolutePath, ulong seed, bool bugged) {
        var lowercaseRelativePath = relativePath.ToLower();
        var basis = 0xcbf29ce484222325UL;
        var prime = 0x00000100000001b3UL;
        if (bugged) { // they swapped prime and basis for a while
            (basis, prime) = (prime, basis);
        }

        basis += seed;

        using var fnv = FowlerNollVo.CreateAlternate((FNV64Basis) basis, prime);

        Paths[fnv.ComputeHashValue(Encoding.UTF8.GetBytes(lowercaseRelativePath))] = absolutePath;
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
        writer.Write(Paths.Count);
        
        foreach (var (hash, path) in Paths) {
            writer.Write(hash);
            writer.Write(path);
        }

        return writer.Data;
    }
}
