using System.Windows;
using System.Windows.Input;
using RaceOverlay.App.ViewModels;

namespace RaceOverlay.App;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        PreviewKeyDown += MainWindow_PreviewKeyDown;
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F1 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            _viewModel.ToggleSetupModeCommand.Execute(null);
            e.Handled = true;
        }
    }

    public MainWindowViewModel GetViewModel() => _viewModel;
}
