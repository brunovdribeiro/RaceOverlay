using System.Reflection;
using System.Windows;
using System.Windows.Input;
using RaceOverlay.App.ViewModels;
using Forms = System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace RaceOverlay.App;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;
    private Forms.NotifyIcon? _notifyIcon;
    private bool _isExiting;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        Title = $"RaceOverlay v{version}";

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        PreviewKeyDown += MainWindow_PreviewKeyDown;
        InitializeTrayIcon();
    }

    private void InitializeTrayIcon()
    {
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = CreateTrayIcon(),
            Text = Title,
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => RestoreFromTray();

        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add("Open RaceOverlay", null, (_, _) => RestoreFromTray());
        contextMenu.Items.Add(new Forms.ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, (_, _) => ExitApplication());
        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    private static System.Drawing.Icon CreateTrayIcon()
    {
        using var bitmap = new System.Drawing.Bitmap(16, 16);
        using var g = System.Drawing.Graphics.FromImage(bitmap);
        var purple = System.Drawing.Color.FromArgb(109, 40, 217);
        g.Clear(purple);
        g.FillEllipse(System.Drawing.Brushes.White, 3, 3, 10, 10);
        g.FillEllipse(new System.Drawing.SolidBrush(purple), 5, 5, 6, 6);
        var handle = bitmap.GetHicon();
        return System.Drawing.Icon.FromHandle(handle);
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExiting)
        {
            e.Cancel = true;
            Hide();
        }
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _isExiting = true;
        _viewModel.SaveConfiguration();
        _notifyIcon?.Dispose();
        _notifyIcon = null;
        Application.Current.Shutdown();
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
