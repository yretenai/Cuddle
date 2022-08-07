using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Headless.Mode;

public static class ListMode {
    public static void Do(CuddleFlags flags, VFSManager manager) {
        foreach (var (_, file) in manager.UniqueFilesPath) {
            if (flags.FullInfo) {
                Log.Information("\t{Name}\t{ObjectName}\t{Hash:x16}\t{Size}\t{Owner}", file.MountedPath, file.ObjectPath, file.MountedHash, file.Size, file.Owner.Name);
            } else {
                Log.Information("\t{Name}\t{Owner}", flags.ObjectPath ? file.ObjectPath : file.MountedPath, file.Owner.Name);
            }
        }
    }
}
