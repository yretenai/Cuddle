using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cuddle.Core.Structs;
using DragonLib.CommandLine;

namespace Cuddle.Headless;

public partial record CuddleFlags : CommandLineFlags {
    [Flag("aes", Aliases = new[] { "k" }, Help = "AES 0xKeyGuid=0xValue for the packages", Category = "Cuddle")]
    public List<string> Keys { get; set; } = new();

    [Flag("game", Help = "Unreal Version to use", Default = EGame.UE4_MAX, Category = "Cuddle", EnumPrefix = new[] { "GAME_", "UE" }, ReplaceDashes = '_', ReplaceDots = '_')]
    public EGame Game { get; set; } = EGame.UE4_MAX;

    [Flag("mode", Default = CuddleMode.Export, Help = "Operation mode", Category = "Cuddle", Hidden = true)]
    public CuddleMode Mode { get; set; } = CuddleMode.Export;

    [Flag("profiling", Hidden = true)]
    public bool Profiling { get; set; }

    [Flag("filter", Help = "Path filters", Category = "Cuddle")]
    public List<Regex> Filters { get; set; } = new();

    [Flag("oodle", Help = "Directory or path to a valid oodle library", Category = "Cuddle")]
    public string? OodlePath { get; set; }

    [Flag("pak-path", IsRequired = true, Positional = 0, Help = "Path to where the packages are", Category = "Cuddle")]
    public string PakPath { get; set; } = null!;

    [Flag("output-path", IsRequired = true, Positional = 1, Help = "Path to where to save files", Category = "Cuddle")]
    public string OutputPath { get; set; } = null!;
}
