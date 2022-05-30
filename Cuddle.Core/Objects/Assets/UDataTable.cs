using System.Collections.Generic;
using System.Linq;
using Cuddle.Core.Assets;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.Structs.Property;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Objects.Assets;

[ObjectRegistration(Expression = ".*DataTable")]
public class UDataTable : UObject {
    public UDataTable(FArchiveReader data, FObjectExport export) : base(data, export) {
        var objectRef = Properties.Values.FirstOrDefault() as ObjectProperty;
        var structName = objectRef?.Value.Reference?.ObjectName.Value;
        var count = data.Read<int>();
        for (var i = 0; i < count; ++i) {
            var rowName = new FName(data);
            Rows[rowName] = ReadProperties(data, FPropertyTagContext.Empty with { ElementTag = objectRef?.Tag ?? FPropertyTag.Empty }, structName ?? export.ObjectName.Value);
        }
    }

    public Dictionary<FName, Dictionary<FPropertyTag, UProperty?>> Rows { get; } = new();
}
