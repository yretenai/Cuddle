using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cuddle.Core;
using Cuddle.Core.VFS;
using Cuddle.Headless.Mode;
using DragonLib.CommandLine;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Cuddle.Headless;

public static class Program {
    public static void Main() {
        var flags = CommandLineFlagsParser.ParseFlags<CuddleFlags>();
        if (flags == null) {
            CommandLineFlagsParser.PrintHelp<CuddleFlags>(CommandLineFlagsParser.PrintHelp, true);
            return;
        }

        Console.OutputEncoding = Encoding.UTF8;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Logs", $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.log"), encoding: Encoding.UTF8)
            .CreateLogger();
        SystemManagement.DescribeLog();
        Log.Debug("{Flags}", flags.ToString());
        Log.Debug("Args: {Args}", string.Join(' ', Environment.GetCommandLineArgs()[1..]));

        using var manager = new VFSManager();
        manager.MountDir(new DirectoryInfo(flags.PakPath), flags.Game);
        manager.Freeze(flags.IsCaseInsensitive);
        if (!string.IsNullOrEmpty(flags.OodlePath)) {
            manager.Oodle = new Oodle(flags.OodlePath);
        }

        if (flags.Cultures.Any()) {
            foreach (var culture in flags.Cultures.Select(x => x.ToCulture())) {
                manager.Culture.LoadCulture(culture);
            }
        }

        switch (flags.Mode) {
            case CuddleMode.Export:
                ExportMode.Do(flags, manager);
                break;
            case CuddleMode.List:
                ListMode.Do(flags, manager);
                break;
            case CuddleMode.DEBUG_Profiling:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
