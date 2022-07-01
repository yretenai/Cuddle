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

public record FMaterialInput<T> : FExpressionInput where T : unmanaged {
    public FMaterialInput(FArchiveReader reader) : base(reader) {
        UseConstantValue = reader.ReadBoolean();
        Constant = reader.Read<T>();
    }

    public bool UseConstantValue { get; set; }
    public T Constant { get; set; }
}
