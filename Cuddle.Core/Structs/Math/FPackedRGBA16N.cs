using Cuddle.Core.Objects;

namespace Cuddle.Core.Structs.Math;

[ObjectRegistration]
public record struct FPackedRGBA16N(int XY, int ZW) {
    public FPackedRGBA16N() : this(0, 0) { }
}
