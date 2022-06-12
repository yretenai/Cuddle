using Cuddle.Core.VFS;

namespace Cuddle.Core;

public interface IArchiveSerializable {
    void Serialize(FArchiveWriter writer);
}
