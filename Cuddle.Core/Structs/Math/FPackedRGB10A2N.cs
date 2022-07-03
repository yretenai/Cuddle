using Cuddle.Core.Objects;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration]
public record struct FPackedRGB10A2N(int Packed) {
    public FPackedRGB10A2N() : this(0) { }
}
