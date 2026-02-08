using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RaceOverlay.App.Services;
using RaceOverlay.App.ViewModels;

namespace RaceOverlay.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
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
        // Register hotkey handler for CTRL+F12
        PreviewKeyDown += MainWindow_PreviewKeyDown;
        
        // Update drag mode indicator initially and on changes
        UpdateDragModeIndicator();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Check for CTRL+F12
        if (e.Key == Key.F12 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            WidgetDragService.Instance.ToggleDragging();
            UpdateDragModeIndicator();

            // Save positions when drag mode is disabled (positions finalized)
            if (!WidgetDragService.Instance.IsDraggingEnabled)
            {
                _viewModel.SaveConfiguration();
            }

            e.Handled = true;
        }
    }

    private void UpdateDragModeIndicator()
    {
        bool isDraggingEnabled = WidgetDragService.Instance.IsDraggingEnabled;
        
        // Update the indicator ellipse color
        if (DragModeIndicator != null)
        {
            if (isDraggingEnabled)
            {
                // Green for enabled
                DragModeIndicator.Fill = (Brush)FindResource("RO.GreenBrush");
            }
            else
            {
                // Red for disabled
                DragModeIndicator.Fill = (Brush)FindResource("RO.RedBrush");
            }
        }
        
        // Update the text
        if (DragModeText != null)
        {
            DragModeText.Text = isDraggingEnabled ? "ON" : "OFF";
        }
    }

    /// <summary>
    /// Gets the main view model instance (accessible to other windows).
    /// </summary>
    public MainWindowViewModel GetViewModel() => _viewModel;
}