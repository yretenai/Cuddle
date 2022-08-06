using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cuddle.Core.Structs.Intl;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Objects.Intl;

public class FTextLocalizationResource {
    public FTextLocalizationResource(FArchiveReader reader) {
        var guid = reader.Read<Guid>();
        if (guid == Magic) {
            Version = reader.Read<ELocResVersion>();
        } else {
            Version = ELocResVersion.Legacy;
            reader.Position = 0;
        }

        var strings = new Collection<string>();
        if (Version >= ELocResVersion.Compact) {
            var offset = reader.Read<long>();
            if (offset > 0) {
                var current = reader.Position;
                reader.Position = (int) offset;
                var count = reader.Read<int>();
                for (var i = 0; i < count; ++i) {
                    strings.Add(reader.ReadString());
                    if (Version >= ELocResVersion.CRC32) {
                        reader.Position += 4; // skip RefCount as it's virtually useless here, and would get rebuilt on save anyway.
                    }
                }

                reader.Position = current;
            }
        }

        if (Version >= ELocResVersion.CRC32) {
            reader.Position += 4; // total entry count, redundant for us since we're double mapping.
        }

        var namespaceCount = reader.Read<int>();
        for (var i = 0; i < namespaceCount; ++i) {
            var @namespace = new FTextKey(Version >= ELocResVersion.CRC32 ? reader.Read<uint>() : 0, reader.ReadString());
            var entryCount = reader.Read<int>();
            var entries = new Dictionary<FTextKey, FTextKey>(entryCount);
            for (var j = 0; j < entryCount; ++j) {
                var key = new FTextKey(Version >= ELocResVersion.CRC32 ? reader.Read<uint>() : 0, reader.ReadString());
                var hash = reader.Read<uint>();
                if (Version >= ELocResVersion.Compact) {
                    entries[key] = new FTextKey(hash, strings[reader.Read<int>()]);
                } else {
                    entries[key] = new FTextKey(hash, reader.ReadString());
                }
            }

            Entries[@namespace] = entries;
        }
    }

    public ELocResVersion Version { get; }
    public static Guid Magic { get; } = new(new byte[] { 0x0E, 0x14, 0x74, 0x75, 0x67, 0x4A, 0x03, 0xFC, 0x4A, 0x15, 0x90, 0x9D, 0xC3, 0x37, 0x7F, 0x1B });

    // double map since we don't have a fancy bucketing technique to improve lookup time, so this is a workaround.
    public Dictionary<FTextKey, Dictionary<FTextKey, FTextKey>> Entries { get; } = new();
}
