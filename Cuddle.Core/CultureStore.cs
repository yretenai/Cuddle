using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Cuddle.Core.Objects.Intl;
using Cuddle.Core.Structs.Intl;
using Cuddle.Core.VFS;

namespace Cuddle.Core;

public class CultureStore {
    public CultureStore(VFSManager manager) => Manager = manager;

    public Dictionary<string, Dictionary<string, string>> StringMap { get; } = new();
    public ECulture PrimaryCulture { get; set; } = ECulture.None;
    public VFSManager Manager { get; set; }

    public void LoadCulture(ECulture culture) {
        if (culture == ECulture.None) {
            return;
        }

        var iso = $"/{culture.ToISO_639_1()}/";

        if (PrimaryCulture == ECulture.None) {
            PrimaryCulture = culture;
        }

        foreach (var locresEntry in Manager.UniqueFilesPath.Where(x => x.ObjectPath.Contains(iso, StringComparison.InvariantCultureIgnoreCase) && x.ObjectPath.EndsWith(".locres", StringComparison.InvariantCultureIgnoreCase))) {
            using var data = locresEntry.ReadFile();
            using var reader = new FArchiveReader(data, Manager);
            var locres = new FTextLocalizationResource(reader);
            ImportLocalizationResource(locres);
        }
    }

    public void Reset() {
        StringMap.Clear();
        PrimaryCulture = ECulture.None;
    }

    public void ImportLocalizationResource(FTextLocalizationResource locres) {
        foreach (var (ns, values) in locres.Entries) {
            if (!StringMap.TryGetValue(ns.Value, out var nsMap)) {
                nsMap = new Dictionary<string, string>();
                StringMap[ns.Value] = nsMap;
            }

            foreach (var (key, value) in values) {
                if (nsMap.ContainsKey(key.Value)) {
                    continue;
                }

                nsMap[key.Value] = value.Value;
            }
        }
    }

    public bool TryGetValue(string ns, string key, [MaybeNullWhen(false)] out string str) {
        if (!StringMap.TryGetValue(ns, out var nsMap)) {
            str = null;
            return false;
        }

        return nsMap.TryGetValue(key, out str);
    }
}
