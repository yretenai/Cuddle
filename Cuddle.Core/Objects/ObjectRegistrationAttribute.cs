using System;
using Cuddle.Core.Structs.Asset;

namespace Cuddle.Core.Objects;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public sealed class ObjectRegistrationAttribute : Attribute {
    public ObjectRegistrationAttribute() { }

    public ObjectRegistrationAttribute(params string[] names) => Names = names;

    public bool Skip { get; set; }
    public string? Expression { get; set; }
    public string[]? Names { get; set; }
    public EObjectVersion MaxVersionUE4 { get; set; } = (EObjectVersion) uint.MaxValue;
    public EObjectVersionUE5 MaxVersionUE5 { get; set; } = (EObjectVersionUE5) uint.MaxValue;
}
