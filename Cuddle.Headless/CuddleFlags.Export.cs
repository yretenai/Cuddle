using DragonLib.CommandLine;

namespace Cuddle.Headless;

public partial record CuddleFlags {
    [Flag("raw", Help = "Dump raw uasset files", Category = "Export")]
    public bool Raw { get; set; }

    [Flag("locres", Help = "Save localization data", Category = "Export")]
    public bool SaveLocRes { get; set; }

    [Flag("no-json", Help = "Suppress JSON generation", Category = "Export")]
    public bool NoJSON { get; set; }
}
