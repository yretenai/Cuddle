using Cuddle.Core.Enums;

namespace Cuddle.Core;

public static class Extensions {
    public static EGame GetEngineVersion(this EGame game) => (EGame) ((uint) game & 0xFFFF0000);
}
