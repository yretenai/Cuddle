using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cuddle.Core.Json;
using Cuddle.Core.VFS;
using DragonLib;
using Serilog;

namespace Cuddle.Headless.Mode;

public static class ExtractMode {
    public static void Do(CuddleFlags flags, VFSManager manager) {
        foreach (var file in manager.UniqueFilesPath) {
            try {
                if (flags.Filters.Count > 0 && !flags.Filters.Any(x => x.IsMatch(file.MountedPath))) {
                    continue;
                }

                var result = Path.Combine(flags.OutputPath, file.MountedPath[0] == '/' ? file.MountedPath[1..] : file.MountedPath);
                var ext = Path.GetExtension(file.MountedPath).ToLower();
                if (ext is ".uasset" or ".umap") {
                    try {
                        using var asset = file.ReadAsset();
                        if (asset == null) {
                            continue;
                        }

                        Log.Information("{Path} -> uasset", file.MountedPath);

                        if (!flags.NoJSON) {
                            var objects = asset.GetExports();
                            var json = JsonSerializer.Serialize(objects, JsonSettings.Options);
                            result.EnsureDirectoryExists();
                            File.WriteAllText(Path.ChangeExtension(result, ".json"), json);
                        }
                    } catch (Exception e) {
                        Log.Error(e, "Failed to process uasset {Path}", file.MountedPath);
                    }
                }

                if (flags.Raw) {
                    using var data = file.ReadFile();
                    if (data.Length == 0) {
                        continue;
                    }

                    Log.Information("{Path} -> Raw", file.MountedPath);
                    result.EnsureDirectoryExists();
                    using var stream = File.OpenWrite(result);
                    stream.SetLength(0);
                    stream.Write(data.Span);
                }
            } catch (Exception e) {
                Log.Error(e, "Failure processing {Path}", file.ObjectPath);
            }
        }
    }
}
