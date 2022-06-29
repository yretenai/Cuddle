using System;
using System.Collections.Generic;
using System.IO;
using Cuddle.Core.Structs;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Cuddle.Core.VFS;

public sealed class FIoStore : IVFSFile {
    public FIoStore(string fullPath, EGame game, string name, AESKeyStore? keyStore, HashPathStore? hashStore, VFSManager manager) {
        Name = name;
        Manager = manager;

        using var casStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var tocStream = new FileStream(Path.ChangeExtension(fullPath, "utoc"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var pakPath = Path.ChangeExtension(fullPath, "utoc");
        if (File.Exists(pakPath)) {
            PakFile = new FPakFile(pakPath, game, name, keyStore, hashStore, manager);
        }

        Game = PakFile?.Game ?? game;

        Toc = new FIoToc(this, tocStream);
    }

    public FPakFile? PakFile { get; set; }
    public FIoToc Toc { get; set; }
    public bool IsGlobal { get; set; }
    public VFSManager Manager { get; }

    public string Name { get; }
    public EGame Game { get; }
    public Guid EncryptionGuid => Toc.EncryptionGuid == Guid.Empty ? PakFile?.EncryptionGuid ?? Guid.Empty : Toc.EncryptionGuid;

    public byte[]? EncryptionKey {
        get => PakFile?.EncryptionKey;
        set {
            if (PakFile != null) {
                PakFile.EncryptionKey = value;
            }
        }
    }

    public bool HasHashes { get; }
    public bool HasPaths { get; }
    public IEnumerable<IVFSEntry> Entries { get; }
    public MemoryOwner<byte> ReadFile(string path) => throw new NotImplementedException();

    public MemoryOwner<byte> ReadFile(ulong hash) => throw new NotImplementedException();

    public MemoryOwner<byte> ReadFile(IVFSEntry entry) => throw new NotImplementedException();

    public MemoryOwner<byte> ReadBytes(long offset, long count, bool isEncrypted) => throw new NotImplementedException();

    public bool FindEncryptionKey(AESKeyStore aesKey, MemoryOwner<byte> test) => throw new NotImplementedException();

    public void ClearCaches() {
        PakFile?.ClearCaches();
    }

    public void Dispose() {
        PakFile?.Dispose();
        Disposed = true;
    }

    public bool Disposed { get; private set; }
}
