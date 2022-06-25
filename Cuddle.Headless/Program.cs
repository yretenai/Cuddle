using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cuddle.Core;
using Cuddle.Core.Json;
using Cuddle.Core.VFS;
using DragonLib;
using DragonLib.CommandLine;
using Serilog;

namespace Cuddle.Headless;

public static class Program {
    public static void Main() {
        var flags = CommandLineFlagsParser.ParseFlags<CuddleFlags>();
        if (flags == null) {
            return;
        }

        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().CreateLogger();
        Oodle.Load(flags.OodlePath);

        using var manager = new VFSManager();
        manager.MountDir(new DirectoryInfo(flags.PakPath), flags.Game);
        if (flags.Profiling) {
            return;
        }

        foreach (var file in manager.UniqueFilesPath) {
            try {
                if (flags.Filters.Count > 0 && !flags.Filters.Any(x => x.IsMatch(file.MountedPath))) {
                    continue;
                }

                var result = Path.Combine(flags.OutputPath, file.MountedPath[0] == '/' ? file.MountedPath[1..] : file.MountedPath);
                var ext = Path.GetExtension(file.MountedPath).ToLower();
                if (ext == ".uasset") {
                    using var asset = file.ReadAsset();
                    if (asset == null) {
                        continue;
                    }

                    Console.WriteLine(file.MountedPath);

                    if (!flags.NoJSON) {
                        var objects = asset.GetExports();
                        var json = JsonSerializer.Serialize(objects, JsonSettings.Options);
                        result.EnsureDirectoryExists();
                        File.WriteAllText(Path.ChangeExtension(result, ".json"), json);
                    }
                }
            } catch (Exception e) {
                Log.Error(e, "Failure deserializing {Path}", file.ObjectPath);
            }
        }
    }
}
