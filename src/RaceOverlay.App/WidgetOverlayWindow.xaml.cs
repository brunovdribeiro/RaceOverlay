using System.Windows;
using RaceOverlay.App.ViewModels;
using RaceOverlay.App.Services;
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
    private bool _isDragging;
    private Point _dragStartPoint;
    private bool _isDraggingEnabled;

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
    /// When window is loaded, populate it with the widget content and register for drag management.
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

        // Register this window for drag management
        WidgetDragService.Instance.RegisterWindow(this);

        // Set initial dragging state
        SetDraggingEnabled(WidgetDragService.Instance.IsDraggingEnabled);
    }

    /// <summary>
    /// Enables or disables dragging for this window.
    /// </summary>
    public void SetDraggingEnabled(bool enabled)
    {
        _isDraggingEnabled = enabled;
        
        if (enabled)
        {
            // Enable drag mode - add event handlers
            WidgetContent.MouseLeftButtonDown += WidgetContent_MouseLeftButtonDown;
            WidgetContent.MouseMove += WidgetContent_MouseMove;
            WidgetContent.MouseLeftButtonUp += WidgetContent_MouseLeftButtonUp;
            WidgetContent.Cursor = System.Windows.Input.Cursors.Hand;
        }
        else
        {
            // Disable drag mode - remove event handlers
            WidgetContent.MouseLeftButtonDown -= WidgetContent_MouseLeftButtonDown;
            WidgetContent.MouseMove -= WidgetContent_MouseMove;
            WidgetContent.MouseLeftButtonUp -= WidgetContent_MouseLeftButtonUp;
            WidgetContent.Cursor = System.Windows.Input.Cursors.Arrow;
            _isDragging = false;
        }
    }

    private void WidgetContent_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_isDraggingEnabled)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(null);
            WidgetContent.CaptureMouse();
        }
    }

    private void WidgetContent_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_isDragging && _isDraggingEnabled)
        {
            Point currentPosition = e.GetPosition(null);
            double deltaX = currentPosition.X - _dragStartPoint.X;
            double deltaY = currentPosition.Y - _dragStartPoint.Y;

            Left += deltaX;
            Top += deltaY;

            _dragStartPoint = currentPosition;
        }
    }

    private void WidgetContent_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            WidgetContent.ReleaseMouseCapture();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Unregister from drag service
        WidgetDragService.Instance.UnregisterWindow(this);
        base.OnClosed(e);
    }
}
