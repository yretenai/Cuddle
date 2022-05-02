using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cuddle.Core.Enums;
using Cuddle.Core.Structs.FileSystem;
using DragonLib.Text;

namespace Cuddle.Core;

public sealed class PakManager : IDisposable {
    public AESKeyStore KeyStore { get; } = new();
    public HashPathStore HashStore { get; } = new();

    public List<UPakFile> Paks { get; } = new();
    public IEnumerable<FPakEntry> Files => Paks.SelectMany(x => x.Index.Files);
    public IEnumerable<FPakEntry> UniqueFiles => Files.DistinctBy(x => x.MountedPath);

    public void MountPakDir(DirectoryInfo dir, EGame game) {
        foreach (var pakPath in dir.GetFiles("*.pak", SearchOption.TopDirectoryOnly).OrderBy(x => x.Name.Replace('.', '_'), new NaturalStringComparer(StringComparison.OrdinalIgnoreCase, true))) {
            Paks.Add(new UPakFile(new FileStream(pakPath.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), game, Path.GetFileNameWithoutExtension(pakPath.Name), KeyStore, HashStore));
        }
    }

    public void Dispose() {
        foreach (var pak in Paks) {
            pak.Dispose();
        }

        Paks.Clear();
    }
}
