using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RaceOverlay.App.ViewModels;
using RaceOverlay.App.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Factories;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Cursors = System.Windows.Input.Cursors;

namespace RaceOverlay.App;

/// <summary>
/// UserControl that hosts a single widget inside the overlay canvas.
/// Uses the factory pattern to create views/viewmodels without type-checking.
/// </summary>
public partial class WidgetHostPanel : UserControl
{
    private bool _isDraggingEnabled;
    private bool _isMouseDragging;
    private Point _dragStartMouse;
    private double _dragStartLeft;
    private double _dragStartTop;

    private IWidgetViewFactory? _factory;
    private object? _viewModel;
    private Action? _unsubscribe;

    public IWidget Widget { get; set; } = null!;
    public string InstanceId { get; set; } = "";
    public MainWindowViewModel ViewModel { get; set; } = null!;
    public WidgetViewFactoryRegistry FactoryRegistry { get; set; } = null!;

    public WidgetHostPanel()
    {
        InitializeComponent();
        Loaded += WidgetHostPanel_Loaded;
    }

    private void WidgetHostPanel_Loaded(object sender, RoutedEventArgs e)
    {
        _factory = FactoryRegistry.GetFactory(Widget.WidgetId);
        if (_factory == null) return;

        var view = _factory.CreateView();
        _viewModel = _factory.CreateViewModel(Widget);
        view.DataContext = _viewModel;
        WidgetContent.Content = view;

        _unsubscribe = _factory.Subscribe(_viewModel, Widget, action => Dispatcher.Invoke(action));

        // Register this panel for drag management
        WidgetDragService.Instance.RegisterPanel(this);
        SetDraggingEnabled(WidgetDragService.Instance.IsDraggingEnabled);
    }

    /// <summary>
    /// Applies a configuration object to the hosted viewmodel via the factory.
    /// </summary>
    public void ApplyConfig(IWidgetConfiguration config)
    {
        if (_factory != null && _viewModel != null)
            _factory.ApplyConfiguration(_viewModel, config);
    }

    // --- Drag mode ---

    public void SetDraggingEnabled(bool enabled)
    {
        _isDraggingEnabled = enabled;

        if (enabled)
        {
            _unsubscribe?.Invoke();
            _unsubscribe = null;
            WidgetContent.Visibility = Visibility.Hidden;
            DragOverlay.Visibility = Visibility.Visible;
            DragOverlay.Background = new SolidColorBrush(Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF));
            DragOverlay.MouseLeftButtonDown += DragOverlay_MouseLeftButtonDown;
            DragOverlay.MouseMove += DragOverlay_MouseMove;
            DragOverlay.MouseLeftButtonUp += DragOverlay_MouseLeftButtonUp;
            DragOverlay.Cursor = Cursors.SizeAll;
        }
        else
        {
            DragOverlay.Visibility = Visibility.Collapsed;
            DragOverlay.Background = Brushes.Transparent;
            WidgetContent.Visibility = Visibility.Visible;
            DragOverlay.MouseLeftButtonDown -= DragOverlay_MouseLeftButtonDown;
            DragOverlay.MouseMove -= DragOverlay_MouseMove;
            DragOverlay.MouseLeftButtonUp -= DragOverlay_MouseLeftButtonUp;
            DragOverlay.Cursor = Cursors.Arrow;

            if (_factory != null && _viewModel != null)
            {
                _factory.RefreshData(_viewModel, Widget);
                _unsubscribe = _factory.Subscribe(_viewModel, Widget, action => Dispatcher.Invoke(action));
            }
        }
    }

    private void DragOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isDraggingEnabled) return;

        _isMouseDragging = true;
        var canvas = Parent as Canvas;
        if (canvas == null) return;

        _dragStartMouse = e.GetPosition(canvas);
        _dragStartLeft = Canvas.GetLeft(this);
        _dragStartTop = Canvas.GetTop(this);

        if (double.IsNaN(_dragStartLeft)) _dragStartLeft = 0;
        if (double.IsNaN(_dragStartTop)) _dragStartTop = 0;

        DragOverlay.CaptureMouse();
        e.Handled = true;
    }

    private void DragOverlay_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isMouseDragging) return;

        var canvas = Parent as Canvas;
        if (canvas == null) return;

        var currentMouse = e.GetPosition(canvas);
        var offsetX = currentMouse.X - _dragStartMouse.X;
        var offsetY = currentMouse.Y - _dragStartMouse.Y;

        var newLeft = _dragStartLeft + offsetX;
        var newTop = _dragStartTop + offsetY;

        Canvas.SetLeft(this, newLeft);
        Canvas.SetTop(this, newTop);
    }

    private void DragOverlay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isMouseDragging) return;

        _isMouseDragging = false;
        DragOverlay.ReleaseMouseCapture();

        var left = Canvas.GetLeft(this);
        var top = Canvas.GetTop(this);
        ViewModel?.SaveWidgetPosition(Widget?.WidgetId, left, top);
        e.Handled = true;
    }

    /// <summary>
    /// Cleans up event subscriptions and unregisters from drag service.
    /// Called when the panel is removed from the overlay host.
    /// </summary>
    public void Cleanup()
    {
        _unsubscribe?.Invoke();
        _unsubscribe = null;
        WidgetDragService.Instance.UnregisterPanel(this);
    }
}
