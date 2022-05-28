using System.IO;
using Cuddle.Core;
using Cuddle.Core.Enums;
using Serilog;

namespace Cuddle.Headless;

public static class Program {
    public static void Main(string[] args) {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        using var manager = new VFSManager();
        manager.MountPakDir(new DirectoryInfo(args[0]), EGame.UE4_26);
    }
}
