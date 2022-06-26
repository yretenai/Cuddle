using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Headless.Mode;

public static class ListMode {
    public static void Do(CuddleFlags flags, VFSManager manager) {
        foreach (var file in manager.UniqueFilesPath) {
            Log.Information("{Name}\t{Object}\t{FSName}", file.MountedPath, file.ObjectPath, file.Owner.Name);
        }
    }
}
