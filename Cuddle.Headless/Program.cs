using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cuddle.Core;
using Cuddle.Core.Enums;
using Cuddle.Core.Json;
using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Headless;

public static class Program {
    public static void Main(string[] args) {
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();
        Oodle.Load(args[2]);
        using var manager = new VFSManager();
        manager.MountPakDir(new DirectoryInfo(args[0]), Enum.Parse<EGame>(args[1]));
        foreach (var file in manager.UniqueFilesPath.Where(x => x.MountedPath.EndsWith(".uasset") && !x.MountedPath.Contains("/Audio/"))) {
            try {
                using var asset = file.ReadAsset();
                if (asset == null) {
                    continue;
                }

                var objects = asset.GetExports();
                var test = JsonSerializer.Serialize(objects, JsonSettings.Options);
            } catch (Exception e) {
                Log.Error(e, "Failure deserializing {Path}", file.ObjectPath);
            }
        }
    }
}
