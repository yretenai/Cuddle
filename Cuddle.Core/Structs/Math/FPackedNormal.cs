using Cuddle.Core.Objects;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration]
public record struct FPackedNormal(byte X, byte Y, byte Z, byte W) {
    public FPackedNormal() : this(0, 0, 0, 0) { }
}
