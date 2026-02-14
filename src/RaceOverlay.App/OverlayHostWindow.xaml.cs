using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace RaceOverlay.App;

/// <summary>
/// Single fullscreen transparent window that hosts all widget panels on a Canvas.
/// Transparent areas are click-through; only widget panels receive mouse input.
/// </summary>
public partial class OverlayHostWindow : Window
{
    public OverlayHostWindow()
    {
        InitializeComponent();
        Loaded += OverlayHostWindow_Loaded;
    }

    private void OverlayHostWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Size to primary screen
        Left = 0;
        Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;

        // Hide from Alt-Tab using WS_EX_TOOLWINDOW
        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE,
            exStyle | NativeMethods.WS_EX_TOOLWINDOW);
    }

    /// <summary>
    /// Adds a widget panel to the canvas at the specified position.
    /// </summary>
    public void AddWidget(WidgetHostPanel panel, double left, double top)
    {
        Canvas.SetLeft(panel, left);
        Canvas.SetTop(panel, top);
        WidgetCanvas.Children.Add(panel);
    }

    /// <summary>
    /// Removes a widget panel from the canvas after cleaning up its resources.
    /// </summary>
    public void RemoveWidget(WidgetHostPanel panel)
    {
        panel.Cleanup();
        WidgetCanvas.Children.Remove(panel);
    }

    /// <summary>
    /// Gets whether any widget panels are currently hosted.
    /// </summary>
    public bool HasWidgets => WidgetCanvas.Children.Count > 0;
}
