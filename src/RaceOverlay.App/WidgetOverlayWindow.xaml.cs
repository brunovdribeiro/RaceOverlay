using System.Windows;
using RaceOverlay.App.ViewModels;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.App;

/// <summary>
/// Overlay window that displays a widget on top of other applications.
/// </summary>
public partial class WidgetOverlayWindow : Window
{
    public static readonly DependencyProperty WidgetProperty =
        DependencyProperty.Register(nameof(Widget), typeof(IWidget), typeof(WidgetOverlayWindow));

    public static readonly DependencyProperty InstanceIdProperty =
        DependencyProperty.Register(nameof(InstanceId), typeof(string), typeof(WidgetOverlayWindow));

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(MainWindowViewModel), typeof(WidgetOverlayWindow));

    /// <summary>
    /// Gets or sets the widget instance to display.
    /// </summary>
    public IWidget Widget
    {
        get => (IWidget)GetValue(WidgetProperty);
        set => SetValue(WidgetProperty, value);
    }

    /// <summary>
    /// Gets or sets the unique instance ID for this widget.
    /// </summary>
    public string InstanceId
    {
        get => (string)GetValue(InstanceIdProperty);
        set => SetValue(InstanceIdProperty, value);
    }

    /// <summary>
    /// Gets or sets the main view model for widget management.
    /// </summary>
    public MainWindowViewModel ViewModel
    {
        get => (MainWindowViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public WidgetOverlayWindow()
    {
        InitializeComponent();
        Loaded += WidgetOverlayWindow_Loaded;
    }

    /// <summary>
    /// When window is loaded, populate it with the widget content.
    /// </summary>
    private void WidgetOverlayWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Create the appropriate view for the widget
        if (Widget is RelativeOverlay relativeOverlay)
        {
            var view = new RelativeOverlayView();
            var viewModel = new RelativeOverlayViewModel();
            viewModel.LoadRelativeDrivers(relativeOverlay.GetRelativeDrivers());
            view.DataContext = viewModel;
            WidgetContent.Content = view;
        }
    }
}
