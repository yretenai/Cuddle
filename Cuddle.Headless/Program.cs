﻿using System;
using System.IO;
using Cuddle.Core;
using Cuddle.Core.Enums;
using Serilog;

namespace Cuddle.Headless;

public static class Program {
    public static void Main(string[] args) {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        using var manager = new VFSManager();
        manager.MountPakDir(new DirectoryInfo(args[0]), Enum.Parse<EGame>(args[1]));
        using var test = manager.ReadAsset(args[2]);
    }
}
