using System.Collections.Generic;
using Cuddle.Core.Enums;

namespace Cuddle.Core.Structs.FileSystem;

public class FPakIndex {
    public FPakIndex(FArchive archive, UPakFile owner, HashPathStore? hashStore) {
        Owner = owner;

        MountPoint = archive.ReadString();
        if (MountPoint.StartsWith("../../../")) {
            MountPoint = MountPoint[8..];
        }

        if (MountPoint == "None") {
            MountPoint = "";
        }

        if (MountPoint.StartsWith("/")) {
            MountPoint = MountPoint[1..];
        }

        Count = archive.Read<int>();

        if (owner.Version < EPakVersion.PathHashIndex) {
            Files.EnsureCapacity(Count);
            for (var index = 0; index < Count; ++index) {
                var path = archive.ReadString();
                var mounted = MountPoint + path;
                Files.Add(new FPakEntry(archive, Owner, false) { Path = path, MountedPath = mounted });
            }
        } else {
            PathHashSeed = archive.Read<ulong>();

            var hasPathHashIndex = archive.ReadBoolean();
            var hashPathIndexOffset = hasPathHashIndex ? archive.Read<long>() : 0;
            var hashPathIndexSize = hasPathHashIndex ? archive.Read<long>() : 0;
            PathHashIndexHash = hasPathHashIndex ? archive.ReadArray<byte>(0x14).ToArray() : null;

            var hasFullDirectoryIndex = archive.ReadBoolean();
            var fullDirectoryIndexOffset = hasFullDirectoryIndex ? archive.Read<long>() : 0;
            var fullDirectoryIndexSize = hasFullDirectoryIndex ? archive.Read<long>() : 0;
            FullDirectoryIndexHash = hasFullDirectoryIndex ? archive.ReadArray<byte>(0x14).ToArray() : null;

            var encodedData = archive.ReadArray<byte>();
            var encodedReader = new FArchive(archive.Game, encodedData);
            Files.EnsureCapacity(Count);
            var encodedMap = new Dictionary<int, int>(Count);
            for (var index = 0; index < Count; ++index) {
                encodedMap[encodedReader.Position] = index;
                Files.Add(new FPakEntry(encodedReader, Owner, true));
            }

            Files.AddRange(archive.ReadClassArray<FPakEntry>(null, owner, false));

            if (hasFullDirectoryIndex) { // we have paths, yay.
                var dirData = owner.ReadBytes(fullDirectoryIndexOffset, fullDirectoryIndexSize);
                var dirReader = new FArchive(encodedReader.Game, dirData);
                var dirCount = dirReader.Read<int>();
                for (var index = 0; index < dirCount; ++index) {
                    var dirName = dirReader.ReadString();
                    if (!dirName.EndsWith('/')) {
                        dirName += '/';
                    }

                    var fileCount = dirReader.Read<int>();

                    for (var fileIndex = 0; fileIndex < fileCount; ++fileIndex) {
                        var fileName = dirReader.ReadString();
                        var entryLoc = dirReader.Read<int>();
                        switch (entryLoc) {
                            case int.MaxValue or int.MinValue: // Invalid, Unused 
                                continue;
                            case < 0:
                                entryLoc = Count + -(entryLoc + 1);
                                break;
                            default:
                                entryLoc = encodedMap[entryLoc];
                                break;
                        }

                        Files[entryLoc].Path = dirName + fileName;
                        Files[entryLoc].MountedPath = MountPoint + Files[entryLoc].Path;
                        // note: figure out what value gets passed to FPakFile::HashPath and store the value in hashStore.
                    }
                }
            } else if (hasPathHashIndex) { // we only have hashes, which is workable.
                var hashData = owner.ReadBytes(hashPathIndexOffset, hashPathIndexSize);
                var hashReader = new FArchive(encodedReader.Game, hashData);
                var count = hashReader.Read<int>();
                for (var index = 0; index < count; ++index) {
                    var hash = hashReader.Read<ulong>();
                    var entryLoc = hashReader.Read<int>();
                    switch (entryLoc) {
                        case int.MaxValue or int.MinValue: // Invalid, Unused 
                            continue;
                        case < 0:
                            entryLoc = Count + -(entryLoc + 1);
                            break;
                        default:
                            entryLoc = encodedMap[entryLoc];
                            break;
                    }

                    var path = hash.ToString("x8");
                    if (hashStore == null || !hashStore.TryGetPath(hash, out var mountPath)) {
                        mountPath = MountPoint + path;
                    } else {
                        path = mountPath[MountPoint.Length..];
                    }

                    Files[entryLoc].Path = path;
                    Files[entryLoc].MountedPath = mountPath;
                }
            } else { // we have nothing :hollow:
                // realistically, should never happen.
                for (var index = 0; index < Files.Count; index++) {
                    var file = Files[index];
                    file.Path = index.ToString("x8");
                    file.MountedPath = MountPoint + file.Path;
                }
            }
        }
    }

    public UPakFile Owner { get; }

    public string MountPoint { get; }
    public int Count { get; }
    public ulong PathHashSeed { get; } // PathHashSeed = FCrc::StrCrc32(*LowercasePakFilename);
    public byte[]? PathHashIndexHash { get; }
    public byte[]? FullDirectoryIndexHash { get; }
    public List<FPakEntry> Files { get; } = new();
}
