namespace Cuddle.Windows;

/// <summary>Interaction logic for MainWindow.xaml</summary>
public partial class Main {
    public Main() {
        InitializeComponent();

        new GameConfigDialog().ShowDialog();
    }
}
