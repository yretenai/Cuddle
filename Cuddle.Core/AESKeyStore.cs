using System;
using System.Collections.Generic;
using DragonLib;

namespace Cuddle.Core;

public sealed class AESKeyStore {
    internal Dictionary<Guid, byte[]> Keys { get; } = new();
    internal List<byte[]> NullKeys { get; } = new();

    public void AddKey(Guid identifier, byte[] key) {
        Keys[identifier] = key;
    }

    public void AddKey(byte[] key) {
        NullKeys.Add(key);
    }

    public void AddKey(Guid identifier, string key) {
        if (key.StartsWith("0x")) {
            key = key[2..];
        }

        Keys[identifier] = key.ToBytes();
    }

    public void AddKey(string key) {
        if (key.StartsWith("0x")) {
            key = key[2..];
        }

        NullKeys.Add(key.ToBytes());
    }
}
