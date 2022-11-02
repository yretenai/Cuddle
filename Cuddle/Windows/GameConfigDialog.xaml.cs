using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cuddle.Management;
using DragonLib;

namespace Cuddle.Windows;

public partial class GameConfigDialog {
    public GameConfigDialog() {
        InitializeComponent();
    }

    private void RemoveAESEntry(object sender, MouseButtonEventArgs e) {
        if (sender is ListBox { DataContext: GameConfigManager manager, SelectedItem: string key }) {
            manager.KeyStore.RemoveKey(key.ToBytes());
        }
    }

    private void Browse(object sender, RoutedEventArgs e) {
        throw new NotImplementedException();
    }

    private void AddKey(object sender, RoutedEventArgs e) {
        if (KeyInput.Text.Length != 66) {
            MessageBox.Show("AES Key must be 32 characters long", "Invalid AES Key", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!KeyInput.Text.StartsWith("0x")) {
            MessageBox.Show("AES Key must start with 0x", "Invalid AES Key", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!Guid.TryParse(GuidInput.Text, out var guid)) {
            guid = Guid.Empty;
        }


        if (DataContext is GameConfigManager manager) {
            manager.KeyStore.AddKey(guid, KeyInput.Text.ToBytes());
            manager.OnPropertyChanged(nameof(manager.Keys));
        }
    }

    private void ClearKeys(object sender, RoutedEventArgs e) {
        if (DataContext is GameConfigManager manager) {
            manager.KeyStore.Clear();
            manager.OnPropertyChanged(nameof(manager.Keys));
        }
    }
}

