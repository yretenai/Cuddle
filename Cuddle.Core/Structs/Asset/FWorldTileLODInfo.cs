namespace Cuddle.Core.Structs.Asset;

public record struct FWorldTileLODInfo(int RelativeStreamingDistance, float Reserved0, float Reserved1, int Reserved2, int Reserved3) {
    public FWorldTileLODInfo() : this(10000, 0, 0, 0, 0) { }
}
