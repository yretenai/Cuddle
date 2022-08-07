using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.FileSystem;
using Cuddle.Security;
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

        Debug.Assert(Toc.Version >= EIoStoreTocVersion.DirectoryIndex);

        if (Toc.Version >= EIoStoreTocVersion.DirectoryIndex) {
            if (Toc.DirectoryIndexBuffer != null) {
                if (Toc.ContainerFlags.HasFlag(EIoContainerFlags.Encrypted)) {
                    if (keyStore == null || !keyStore.FindEncryptionKey(EncryptionGuid, Toc.DirectoryIndexBuffer.Span[..16], out var enc)) {
                        Log.Error("Can't find encryption key that suits Encryption Key GUID {KeyGuid} for IoStore {StoreName}", EncryptionGuid, Name);
                        return;
                    }

                    EncryptionKey = enc;
                }

                using var directoryReader = new FArchiveReader(Decrypt(Toc.DirectoryIndexBuffer, Toc.ContainerFlags.HasFlag(EIoContainerFlags.Encrypted)));
                Directory = new FIoDirectory(directoryReader, hashStore, this);

                Toc.DirectoryIndexBuffer.Dispose();
                Toc.DirectoryIndexBuffer = null;
            }
        }

        // if there is no directory index, load pak for the FPakIndex. idk if anything will ever use this version.
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

        throw new NotImplementedException();
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
        var size = AESKeyStore.Decrypt(EncryptionKey, data.Span, decryptedOwner.Span);
        data.Dispose();

        return decryptedOwner[..size];
    }
}
