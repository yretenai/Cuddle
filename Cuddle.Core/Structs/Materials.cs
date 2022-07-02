using System;
using Cuddle.Core.Assets;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Structs;

public record FExpressionInput : FFallbackStruct {
    public FExpressionInput(FArchiveReader reader) {
        OutputIndex = reader.Read<int>();
        InputName = new FName(reader);
        Mask = reader.Read<int>();
        MaskR = reader.Read<int>();
        MaskG = reader.Read<int>();
        MaskB = reader.Read<int>();
        MaskA = reader.Read<int>();

        if (!reader.HasEditorData) {
            ExpressionName = new FName(reader);
        }
    }

    public int OutputIndex { get; set; }
    public FName InputName { get; set; }
    public int Mask { get; set; }
    public int MaskR { get; set; }
    public int MaskG { get; set; }
    public int MaskB { get; set; }
    public int MaskA { get; set; }
    public FName ExpressionName { get; set; }
}

public abstract record FMaterialInput<T> : FExpressionInput {
    protected FMaterialInput(FArchiveReader reader) : base(reader) => UseConstantValue = reader.ReadBoolean();

    public bool UseConstantValue { get; set; }
    public T Constant { get; set; } = default!;
}

public record FUnmanagedMaterialInput<T> : FMaterialInput<T> where T : unmanaged {
    public FUnmanagedMaterialInput(FArchiveReader reader) : base(reader) => Constant = reader.Read<T>();
}

public record FManagedMaterialInput<T> : FMaterialInput<T> where T : class, new() {
    public FManagedMaterialInput(FArchiveReader reader) : base(reader) {
        var t = typeof(T);
        Constant = (T) Activator.CreateInstance(t, reader, t.Name[1..])!;
    }
}
