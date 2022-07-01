using Cuddle.Core.Assets;
using Cuddle.Core.Structs;
using Cuddle.Core.VFS;

namespace Cuddle.Core.Objects;

[ObjectRegistration("StringAssetReference", "StringClassReference", "SoftClassPath")]
public record FSoftObjectPath : FStructValue {
    public FSoftObjectPath() {
        Path = FName.Null;
        SubPath = "None";
    }

    public FSoftObjectPath(FArchiveReader data) {
        Path = new FName(data);
        SubPath = data.ReadString();
    }

    public FSoftObjectPath(string str) {
        Path = FName.Null;
        SubPath = str;
    }

    public FName Path { get; }
    public string SubPath { get; }
}
