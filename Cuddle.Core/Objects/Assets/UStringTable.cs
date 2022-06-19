using System.Collections.Generic;
using Cuddle.Core.Assets;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Objects.Assets;

[ObjectRegistration(Expression = ".*StringTable")]
public record UStringTable : UObject {
    public UStringTable(FArchiveReader data, FObjectExport export) : base(data, export) {
        TableNamespace = data.ReadString();
        var entryCount = data.Read<int>();
        for (var i = 0; i < entryCount; ++i) {
            var key = data.ReadString();
            var value = data.ReadString();
            Keys.Add(key, value);
        }

        var metadataEntryCount = data.Read<int>();
        for (var i = 0; i < metadataEntryCount; ++i) {
            var key = data.ReadString();
            var metadataCount = data.Read<int>();
            var entries = new Dictionary<FName, string>();
            for (var j = 0; j < metadataCount; ++j) {
                var prop = new FName(data);
                var value = data.ReadString();
                entries[prop] = value;
            }

            Metadata[key] = entries;
        }
    }

    public Dictionary<string, string> Keys { get; } = new();
    public Dictionary<string, Dictionary<FName, string>> Metadata = new();
    public string TableNamespace { get; set; }
}
