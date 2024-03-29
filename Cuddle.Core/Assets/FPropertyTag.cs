﻿using System;
using Cuddle.Core.Structs;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Assets;

public readonly record struct FPropertyTag {
    public FPropertyTag() { }

    public FPropertyTag(FArchiveReader data, FPropertyTagContext context) {
        // todo: GVAS, no Package but is serialized the same way with FName being FString.

        if (context.IsGVAS) {
            throw new NotImplementedException();
        }

        Name = new FName(data);
        if (Name == "None") {
            return;
        }

        Type = new FName(data);
        Size = data.Read<int>();
        Index = data.Read<int>();

        switch (Type) {
            case "StructProperty":
                ValueType = new FName(data);
                if (data.Version > EObjectVersion.STRUCT_GUID_IN_PROPERTY_TAG) {
                    StructGuid = data.Read<Guid>();
                }

                break;
            case "BoolProperty":
                BoolValue = data.Read<byte>() == 1;
                break;
            case "ByteProperty":
            case "EnumProperty":
                ValueType = new FName(data);
                break;
            case "ArrayProperty" when data.Version > EObjectVersion.ARRAY_PROPERTY_INNER_TAGS:
                ValueType = new FName(data);
                break;
            case "SetProperty" when data.Version > EObjectVersion.PROPERTY_TAG_SET_MAP_SUPPORT:
                ValueType = new FName(data);
                break;
            case "MapProperty" when data.Version > EObjectVersion.PROPERTY_TAG_SET_MAP_SUPPORT:
                KeyType = new FName(data);
                ValueType = new FName(data);
                break;
        }

        if (data.Version > EObjectVersion.PROPERTY_GUID_IN_PROPERTY_TAG) {
            if (data.Read<byte>() == 0x1) {
                Guid = data.Read<Guid>();
            }
        }
    }

    public FName Name { get; init; } = FName.Null;
    public FName Type { get; init; } = FName.Null;
    public int Size { get; init; }

    // multiple properties can have the same Name, Type, and SubTypes, when that happens it increments the ArrayIndex by one.
    // real case: FMaterialCachedParameters, "Entries" is partitioned into multiple FMaterialCachedParameterEntry values
    // they all have the same name (usually "RuntimeEntry" or "Entry") 
    public int Index { get; init; }
    public bool BoolValue { get; }
    public FName KeyType { get; init; } = FName.Null;
    public FName ValueType { get; init; } = FName.Null;
    public Guid StructGuid { get; }
    public Guid Guid { get; }
    public static FPropertyTag Empty { get; } = new();

    public override int GetHashCode() => HashCode.Combine(Name, Type, Size, Index, KeyType, ValueType);

    public FPropertyTag AsValueTag() => new() { Type = ValueType, Name = Name, Size = -1 };

    public FPropertyTag AsKeyTag() => new() { Type = KeyType, Name = Name, Size = -1 };
}
