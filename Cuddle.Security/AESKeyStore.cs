using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using DragonLib;
using Serilog;

namespace Cuddle.Security;

public sealed class AESKeyStore {
    public Dictionary<Guid, byte[]> Keys { get; } = new();
    public List<byte[]> NullKeys { get; } = new();
    public IEnumerable<string> AllKeys => Keys.Values.Concat(NullKeys).Select(x => x.ToHexString()).Distinct();

    public void RemoveKey(Guid identifier) {
        if (Keys.TryGetValue(identifier, out var key)) {
            NullKeys.Remove(key);
        }

        Keys.Remove(identifier);
    }

    public void RemoveKey(byte[] key) {
        NullKeys.Remove(key);
        Keys.Remove(Keys.First(x => x.Value == key).Key);
    }

    public void Clear() {
        NullKeys.Clear();
        Keys.Clear();
    }

    public void AddKey(Guid identifier, byte[] key) {
        if (identifier == Guid.Empty) {
            AddKey(key);
            return;
        }

        Keys[identifier] = key;
    }

    public void AddKey(byte[] key) {
        NullKeys.Add(key);
    }

    public void AddKey(Guid identifier, string key) {
        Keys[identifier] = key.ToBytes();
    }

    public void AddKey(string key) {
        if (key.Contains('=')) {
            var parts = key.Split('=', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            AddKey(parts[0].StartsWith("0x") ? new Guid(parts[0][2..].ToBytes()) : Guid.Parse(parts[0]), parts[1]);
            return;
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

    public bool FindEncryptionKey(Guid encryptionGuid, Span<byte> test, [MaybeNullWhen(false)] out byte[] key) {
        if (Keys.TryGetValue(encryptionGuid, out key)) {
            return true;
        }

        var dec = new byte[16].AsSpan();
        foreach (var unknownKey in NullKeys.Concat(Keys.Values)) {
            Decrypt( unknownKey, new Span<byte>(test.ToArray()), dec);
            if (Math.Abs(BinaryPrimitives.ReadInt32LittleEndian(dec)) < 255) {
                key = unknownKey;
                Keys[encryptionGuid] = key;
                return true;
            }
        }

        key = null;
        return false;
    }


    public static int Decrypt(byte[] key, Span<byte> enc, Span<byte> dec) {
        using var cipher = Aes.Create();
#pragma warning disable CA5358
        cipher.Mode = CipherMode.ECB;
#pragma warning restore CA5358
        cipher.Padding = PaddingMode.None;
        cipher.BlockSize = 128;
        cipher.Key = key;
        cipher.IV = new byte[16];
        return cipher.DecryptEcb(enc, dec, cipher.Padding);
    }
}
