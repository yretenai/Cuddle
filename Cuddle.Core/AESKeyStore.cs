using System;
using System.Collections.Generic;
using DragonLib;
using Serilog;

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
        if (key.Contains('=')) {
            var parts = key.Split('=', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            AddKey(parts[0].StartsWith("0x") ? new Guid(parts[0][2..].ToBytes()) : Guid.Parse(parts[0]), parts[1]);
            return;
        }

        if (key.StartsWith("0x")) {
            key = key[2..];
        }

        NullKeys.Add(key.ToBytes());
    }

    public void Dump() {
        if (Keys.Count == 0 && NullKeys.Count == 0) {
            return;
        }

        Log.Information("Keys:");
        foreach (var key in Keys) {
            Log.Information("  {Guid:n} = 0x{Key}", key.Key, key.Value.ToHexString());
        }

        foreach (var key in NullKeys) {
            Log.Information("  {Guid:n} = 0x{Key}", Guid.Empty, key.ToHexString());
        }
    }
}
