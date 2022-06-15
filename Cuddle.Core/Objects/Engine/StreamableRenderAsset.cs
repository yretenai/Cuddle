using Cuddle.Core.Assets;
using Cuddle.Core.Structs.Asset;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Objects.Engine;

public record UStreamableRenderAsset : UObject {
    public UStreamableRenderAsset(FArchiveReader data, FObjectExport export) : base(data, export) {
        NumCinematicMipLevels = GetProperty<int>(nameof(NumCinematicMipLevels));
        NeverStream = GetProperty<bool>(nameof(NeverStream));
        bGlobalForceMipLevelsToBeResident = GetProperty<bool>(nameof(bGlobalForceMipLevelsToBeResident));
    }
    
    public int NumCinematicMipLevels { get; init; }
    public bool NeverStream { get; init; }
    public bool bGlobalForceMipLevelsToBeResident { get; init; }
}
