using System;

namespace Cuddle.Core.Objects;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ObjectRegistrationAttribute : Attribute {
    public ObjectRegistrationAttribute() { }

    public ObjectRegistrationAttribute(params string[] names) => Names = names;

    public bool Skip { get; set; }
    public string? Expression { get; set; }
    public string[]? Names { get; set; }
}
