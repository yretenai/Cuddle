using System;
using System.IO;
using System.Text.Json;
using Cuddle.Core;
using Cuddle.Core.Enums;
using Cuddle.Core.Json;
using Cuddle.Core.VFS;
using Serilog;

namespace Cuddle.Headless;

public static class Program {
    public static void Main(string[] args) {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        Oodle.Load(args[3]);
        using var manager = new VFSManager();
        manager.MountPakDir(new DirectoryInfo(args[0]), Enum.Parse<EGame>(args[1]));
        var export = manager.ReadExport(args[2], 0);
        File.WriteAllText(args[4], JsonSerializer.Serialize(export, JsonSettings.Options));
    }
}
