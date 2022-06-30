using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.FileSystem;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Serilog;

namespace Cuddle.Core.VFS;

public sealed class FIoStore : IVFSFile {
    public FIoStore(string fullPath, EGame game, string name, AESKeyStore? keyStore, HashPathStore? hashStore, VFSManager manager) {
        Name = name;
        Manager = manager;
        Game = game;

        FullPath = fullPath;
        using var tocStream = new FileStream(Path.ChangeExtension(fullPath, "utoc"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        Toc = new FIoToc(this, tocStream, keyStore);

        if (Toc.DirectoryIndexBuffer != null) {
            if (Toc.ContainerFlags.HasFlag(EIoContainerFlags.Encrypted)) {
                if (keyStore == null || !FindEncryptionKey(keyStore, Toc.DirectoryIndexBuffer.Span[..16])) {
                    Log.Error("Can't find encryption key that suits Encryption Key GUID {KeyGuid} for IoStore {StoreName}", EncryptionGuid, Name);
                    return;
                }
            }

            using var directoryReader = new FArchiveReader(Decrypt(Toc.DirectoryIndexBuffer, Toc.ContainerFlags.HasFlag(EIoContainerFlags.Encrypted)));
            Directory = new FIoDirectory(directoryReader, this);
        }
    }

    public FIoToc Toc { get; set; }
    public FIoDirectory? Directory { get; set; }
    public bool IsGlobal { get; set; }
    public string FullPath { get; }
    public VFSManager Manager { get; }

    public string Name { get; }
    public EGame Game { get; }
    public Guid EncryptionGuid => Toc.EncryptionGuid;
    public byte[]? EncryptionKey { get; set; }
    public bool HasHashes { get; }
    public bool HasPaths { get; }
    public IEnumerable<IVFSEntry> Entries => Directory?.Files ?? new List<FIoFile>();

    public MemoryOwner<byte> ReadFile(string path) {
        var index = Directory?.Files.FirstOrDefault(x => x.MountedPath == path || x.ObjectPath.EndsWith(path, StringComparison.Ordinal));
        return index == null ? MemoryOwner<byte>.Empty : ReadFile(index);
    }

    public MemoryOwner<byte> ReadFile(ulong hash) {
        if (!HasHashes) {
            return MemoryOwner<byte>.Empty;
        }

        var index = Directory?.Files.FirstOrDefault(x => x.MountedHash == hash);
        return index == null ? MemoryOwner<byte>.Empty : ReadFile(index);
    }

    public MemoryOwner<byte> ReadFile(IVFSEntry entry) {
        if (entry is not FIoFile file) {
            throw new InvalidOperationException();
        }

        return MemoryOwner<byte>.Empty;
    }

    public MemoryOwner<byte> ReadBytes(long offset, long count, bool isEncrypted) => throw new NotImplementedException();

    public void ClearCaches() {
        foreach (var entry in Entries) {
            if (entry.Data is IDisposable disposable) {
                disposable.Dispose();
            }

            entry.Data = null;
        }
    }

    public void Dispose() {
        Toc.Dispose();
        Disposed = true;
    }

    public bool Disposed { get; private set; }

    private MemoryOwner<byte> Decrypt(MemoryOwner<byte> data, bool isEncrypted) {
        if (!isEncrypted || EncryptionKey == null) {
            return data;
        }

        var decryptedOwner = MemoryOwner<byte>.Allocate(data.Length);
        var size = DecryptInner(data.Span, decryptedOwner.Span);
        data.Dispose();

        return decryptedOwner[..size];
    }

    private int DecryptInner(Span<byte> enc, Span<byte> dec) {
        using var cipher = Aes.Create();
#pragma warning disable CA5358
        cipher.Mode = CipherMode.ECB;
#pragma warning restore CA5358
        cipher.Padding = PaddingMode.None;
        cipher.BlockSize = 128;
        cipher.Key = EncryptionKey!;
        cipher.IV = new byte[16];
        return cipher.DecryptEcb(enc, dec, cipher.Padding);
    }

    private bool FindEncryptionKey(AESKeyStore aesKey, Span<byte> test) {
        if (EncryptionKey != null) {
            return true;
        }

        if (aesKey.Keys.TryGetValue(EncryptionGuid, out var key)) {
            EncryptionKey = key;
            return true;
        }

        var dec = new byte[16].AsSpan();
        foreach (var unknownKey in aesKey.NullKeys) {
            EncryptionKey = unknownKey;
            DecryptInner(new Span<byte>(test.ToArray()), dec);
            if (Math.Abs(BinaryPrimitives.ReadInt32LittleEndian(dec)) < 255) {
                aesKey.Keys[EncryptionGuid] = EncryptionKey;
                return true;
            }
        }

        EncryptionKey = null;
        return false;
    }
}
