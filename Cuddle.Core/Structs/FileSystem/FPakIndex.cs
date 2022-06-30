using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.FileSystem;

public class FPakIndex {
    public FPakIndex(FArchiveReader archive, FPakFile owner, HashPathStore? hashStore) {
        Owner = owner;

        MountPoint = archive.ReadString();
        OriginalMountPoint = MountPoint;
        if (MountPoint.StartsWith("../../../")) {
            MountPoint = MountPoint[8..];
        }

        if (MountPoint == "None") {
            MountPoint = "";
        }

        if (MountPoint.Length > 1) {
            if (MountPoint.StartsWith("/")) {
                MountPoint = MountPoint[1..];
            }

            if (!MountPoint.EndsWith("/")) {
                MountPoint += '/';
            }
        }

        Count = archive.Read<int>();

        if (owner.Version < EPakVersion.PathHashIndex) {
            Files.EnsureCapacity(Count);
            for (var index = 0; index < Count; ++index) {
                var path = archive.ReadString();
                var mounted = MountPoint + path;
                Files.Add(new FPakEntry(archive, Owner, false) { Path = path, MountedPath = mounted });
            }
        }
        else {
            PathHashSeed = archive.Read<ulong>();

            var hasPathHashIndex = archive.ReadBoolean();
            var hashPathIndexOffset = hasPathHashIndex ? archive.Read<long>() : 0;
            var hashPathIndexSize = hasPathHashIndex ? archive.Read<long>() : 0;
            PathHashIndexHash = hasPathHashIndex ? archive.ReadArray<byte>(0x14).ToArray() : null;

            var hasFullDirectoryIndex = archive.ReadBoolean();
            var fullDirectoryIndexOffset = hasFullDirectoryIndex ? archive.Read<long>() : 0;
            var fullDirectoryIndexSize = hasFullDirectoryIndex ? archive.Read<long>() : 0;
            FullDirectoryIndexHash = hasFullDirectoryIndex ? archive.ReadArray<byte>(0x14).ToArray() : null;

            using var encodedReader = archive.Partition();

            var frozen = archive.ReadClassArray<FPakEntry>(null, owner, false);
            Files.EnsureCapacity(Count + frozen.Length);

            var entryLocCache = new Dictionary<int, int>();

            if (hasFullDirectoryIndex) { // we have paths, yay.
                using var dirReader = new FArchiveReader(encodedReader.Game, owner.ReadBytes(fullDirectoryIndexOffset, fullDirectoryIndexSize, owner.IsIndexEncrypted));
                var dirCount = dirReader.Read<int>();
                for (var index = 0; index < dirCount; ++index) {
                    var dirName = dirReader.ReadString();
                    var fileCount = dirReader.Read<int>();
                    if (dirName[0] == '/') {
                        dirName = dirName[1..];
                    }

                    for (var fileIndex = 0; fileIndex < fileCount; ++fileIndex) {
                        var fileName = dirReader.ReadString();
                        var entryLoc = dirReader.Read<int>();
                        FPakEntry entry;
                        switch (entryLoc) {
                            case int.MaxValue or int.MinValue: // Invalid, Unused
                                continue;
                            case < 0:
                                entry = frozen[-(entryLoc + 1)] with { };
                                break;
                            default: {
                                Debug.Assert(entryLocCache.ContainsKey(entryLoc) == false, "entryLocCache.ContainsKey(entryLoc) == false");
                                encodedReader.Position = entryLoc;
                                entry = new FPakEntry(encodedReader, Owner, true);
                                break;
                            }
                        }

                        entry.Path = dirName + fileName;
                        entry.MountedPath = MountPoint + entry.Path;
                        entry.ObjectPath = FPakEntry.CreateObjectPath(entry.MountedPath);
                        if (entryLoc >= 0) {
                            entryLocCache[entryLoc] = Files.Count;
                        }

                        Files.Add(entry);
                        // note: figure out what value gets passed to FPakFile::HashPath and store the value in hashStore.
                    }
                }
            }

            // we only have hashes, which is workable.
            if (hasPathHashIndex) {
                using var hashReader = new FArchiveReader(encodedReader.Game, owner.ReadBytes(hashPathIndexOffset, hashPathIndexSize, owner.IsIndexEncrypted));
                var count = hashReader.Read<int>();
                for (var index = 0; index < count; ++index) {
                    var hash = hashReader.Read<ulong>();
                    var entryLoc = hashReader.Read<int>();
                    FPakEntry entry;

                    if (!hasFullDirectoryIndex || !entryLocCache.TryGetValue(entryLoc, out var entryIndex)) {
                        switch (entryLoc) {
                            case int.MaxValue or int.MinValue: // Invalid, Unused
                                continue;
                            case < 0:
                                entry = frozen[-(entryLoc + 1)] with { };
                                break;
                            default: {
                                encodedReader.Position = entryLoc;
                                entry = new FPakEntry(encodedReader, Owner, true);
                                break;
                            }
                        }

                        var path = hash.ToString("x8");
                        if (hashStore == null || !hashStore.TryGetPath(hash, out var mountPath)) {
                            mountPath = MountPoint + path; // this is bad.
                        }
                        else {
                            path = mountPath[MountPoint.Length..];
                        }

                        entry.Path = path;
                        entry.MountedPath = mountPath;
                        entry.ObjectPath = FPakEntry.CreateObjectPath(entry.MountedPath);
                        Files.Add(entry);
                    }
                    else {
                        entry = Files[entryIndex];
                    }

                    entry.MountedHash = hash;
                }
            }

            // we have nothing :hollow:
            if (!hasFullDirectoryIndex && !hasPathHashIndex) {
                throw new InvalidDataException();
            }
        }
    }

    public FPakFile Owner { get; }

    public string MountPoint { get; }
    public string OriginalMountPoint { get; }
    public int Count { get; }
    public ulong PathHashSeed { get; } // PathHashSeed = FCrc::StrCrc32(*LowercasePakFilename);
    public byte[]? PathHashIndexHash { get; }
    public byte[]? FullDirectoryIndexHash { get; }
    public List<FPakEntry> Files { get; } = new();
}
