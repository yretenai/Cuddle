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
