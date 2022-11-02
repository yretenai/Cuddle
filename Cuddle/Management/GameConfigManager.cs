using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Cuddle.Core;
using Cuddle.Core.Structs;
using Cuddle.Security;

namespace Cuddle.Management;

public class GameConfigManager : INotifyPropertyChanged {
    public record GameConfigEntry(EGame Game) {
        public string Text => Game.AsFormattedString();
        public Thickness Margin => new(((Game.Minor() > 0 ? 1 : 0) + (Game.IsCustom() ? 1 : 0)) * 7.5d, 0, 0, 0);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public static IEnumerable<GameConfigEntry> GameTargets => Enum.GetValues<EGame>().Distinct().Where(x => x is not (EGame.UE4_MAX or EGame.UE5_MAX)).Select(x => new GameConfigEntry(x));

    public static GameConfigManager Instance { get; } = new();
    public GameConfigEntry GameTarget { get; set; } = new(EGame.UE4_27);
    public AESKeyStore KeyStore { get; } = new();
    public IEnumerable<string> Keys => KeyStore.AllKeys;
}
