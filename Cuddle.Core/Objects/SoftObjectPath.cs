using Cuddle.Core.Assets;
using Cuddle.Core.Structs;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Objects;

[ObjectRegistration("StringAssetReference", "StringClassReference", "SoftClassPath")]
public record SoftObjectPath : FStructValue {
    public SoftObjectPath(FArchiveReader data) {
        Path = new FName(data);
        SubPath = data.ReadString();
    }

    public FName Path { get; }
    public string SubPath { get; }
}
