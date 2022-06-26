using DragonLib.CommandLine;

namespace Cuddle.Headless;

public partial record CuddleFlags {
    [Flag("object-path", Help = "Print object path rather than the game path", Category = "List")]
    public bool ObjectPath { get; set; }

    [Flag("full-info", Help = "Print file full information", Category = "List")]
    public bool FullInfo { get; set; }
}
