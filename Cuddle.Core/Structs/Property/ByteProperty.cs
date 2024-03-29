﻿using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs.Property;

public class ByteProperty : UProperty {
    public ByteProperty(FArchiveReader data, FPropertyTag tag, FPropertyTagContext context) : base(tag, context, data.Asset) {
        Value = context.ReadMode switch {
            FPropertyReadMode.Map => (byte) data.Read<int>(),
            _ => data.Read<byte>(),
        };
    }

    public byte Value { get; }

    public override string ToString() => Value.ToString();
    public override object GetValue() => Value;
}
