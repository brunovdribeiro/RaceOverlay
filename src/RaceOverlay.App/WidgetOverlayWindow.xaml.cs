using System.Windows;
using System.Windows.Media;
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
    private RelativeOverlay? _relativeOverlay;
    private RelativeOverlayViewModel? _viewModel;
    private FuelCalculator? _fuelCalculator;
    private FuelCalculatorViewModel? _fuelCalcViewModel;
    private InputsWidget? _inputsWidget;
    private InputsViewModel? _inputsViewModel;
    private InputTraceWidget? _inputTraceWidget;
    private InputTraceViewModel? _inputTraceViewModel;
    private StandingsWidget? _standingsWidget;
    private StandingsViewModel? _standingsViewModel;

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
        LocationChanged += WidgetOverlayWindow_LocationChanged;
    }

    /// <summary>
    /// When window is loaded, populate it with the widget content and register for drag management.
    /// </summary>
    private void WidgetOverlayWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Create the appropriate view for the widget
        if (Widget is RelativeOverlay relativeOverlay)
        {
            _relativeOverlay = relativeOverlay;
            var view = new RelativeOverlayView();
            _viewModel = new RelativeOverlayViewModel();

            if (relativeOverlay.Configuration is IRelativeOverlayConfig config)
            {
                _viewModel.ApplyConfiguration(config);
            }

            _viewModel.LoadRelativeDrivers(relativeOverlay.GetRelativeDrivers());
            view.DataContext = _viewModel;
            WidgetContent.Content = view;

            // Subscribe to data updates from the widget's update loop
            relativeOverlay.DataUpdated += OnRelativeDataUpdated;
        }
        else if (Widget is FuelCalculator fuelCalc)
        {
            _fuelCalculator = fuelCalc;
            var view = new FuelCalculatorView();
            _fuelCalcViewModel = new FuelCalculatorViewModel();

            if (fuelCalc.Configuration is IFuelCalculatorConfig config)
            {
                _fuelCalcViewModel.ApplyConfiguration(config);
            }

            _fuelCalcViewModel.UpdateFuelData(fuelCalc.GetFuelData());
            view.DataContext = _fuelCalcViewModel;
            WidgetContent.Content = view;

            fuelCalc.DataUpdated += OnFuelDataUpdated;
        }
        else if (Widget is InputsWidget inputsWidget)
        {
            _inputsWidget = inputsWidget;
            var view = new InputsView();
            _inputsViewModel = new InputsViewModel();

            if (inputsWidget.Configuration is IInputsConfig config)
            {
                _inputsViewModel.ApplyConfiguration(config);
            }

            _inputsViewModel.UpdateInputsData(inputsWidget.GetInputsData());
            view.DataContext = _inputsViewModel;
            WidgetContent.Content = view;

            inputsWidget.DataUpdated += OnInputsDataUpdated;
        }
        else if (Widget is InputTraceWidget inputTraceWidget)
        {
            _inputTraceWidget = inputTraceWidget;
            var view = new InputTraceView();
            _inputTraceViewModel = new InputTraceViewModel();

            if (inputTraceWidget.Configuration is IInputTraceConfig config)
            {
                _inputTraceViewModel.ApplyConfiguration(config);
            }

            _inputTraceViewModel.UpdateTrace(inputTraceWidget.GetTraceHistory());
            view.DataContext = _inputTraceViewModel;
            WidgetContent.Content = view;

            inputTraceWidget.DataUpdated += OnInputTraceDataUpdated;
        }
        else if (Widget is StandingsWidget standingsWidget)
        {
            _standingsWidget = standingsWidget;
            var view = new StandingsView();
            _standingsViewModel = new StandingsViewModel();

            if (standingsWidget.Configuration is IStandingsConfig config)
            {
                _standingsViewModel.ApplyConfiguration(config);
            }

            _standingsViewModel.UpdateStandings(standingsWidget.GetStandings(), standingsWidget.CurrentLap, standingsWidget.TotalLaps);
            view.DataContext = _standingsViewModel;
            WidgetContent.Content = view;

            standingsWidget.DataUpdated += OnStandingsDataUpdated;
        }

        // Register this window for drag management
        WidgetDragService.Instance.RegisterWindow(this);

        // Set initial dragging state
        SetDraggingEnabled(WidgetDragService.Instance.IsDraggingEnabled);
    }

    /// <summary>
    /// Applies column visibility settings from configuration to the overlay view model.
    /// </summary>
    public void ApplyColumnVisibility(IRelativeOverlayConfig config)
    {
        _viewModel?.ApplyConfiguration(config);
    }

    public void ApplyFuelConfig(IFuelCalculatorConfig config)
    {
        _fuelCalcViewModel?.ApplyConfiguration(config);
    }

    public void ApplyInputsConfig(IInputsConfig config)
    {
        _inputsViewModel?.ApplyConfiguration(config);
    }

    public void ApplyInputTraceConfig(IInputTraceConfig config)
    {
        _inputTraceViewModel?.ApplyConfiguration(config);
    }

    public void ApplyStandingsConfig(IStandingsConfig config)
    {
        _standingsViewModel?.ApplyConfiguration(config);
    }

    private void OnRelativeDataUpdated()
    {
        if (_relativeOverlay == null || _viewModel == null) return;

        var overlay = _relativeOverlay;
        var vm = _viewModel;
        Dispatcher.Invoke(() => vm.RefreshDrivers(overlay.GetRelativeDrivers()));
    }

    private void OnFuelDataUpdated()
    {
        if (_fuelCalculator == null || _fuelCalcViewModel == null) return;

        var calc = _fuelCalculator;
        var vm = _fuelCalcViewModel;
        Dispatcher.Invoke(() => vm.UpdateFuelData(calc.GetFuelData()));
    }

    private void OnInputsDataUpdated()
    {
        if (_inputsWidget == null || _inputsViewModel == null) return;

        var widget = _inputsWidget;
        var vm = _inputsViewModel;
        Dispatcher.Invoke(() => vm.UpdateInputsData(widget.GetInputsData()));
    }

    private void OnInputTraceDataUpdated()
    {
        if (_inputTraceWidget == null || _inputTraceViewModel == null) return;

        var widget = _inputTraceWidget;
        var vm = _inputTraceViewModel;
        Dispatcher.Invoke(() => vm.UpdateTrace(widget.GetTraceHistory()));
    }

    private void OnStandingsDataUpdated()
    {
        if (_standingsWidget == null || _standingsViewModel == null) return;

        var widget = _standingsWidget;
        var vm = _standingsViewModel;
        Dispatcher.Invoke(() => vm.UpdateStandings(widget.GetStandings(), widget.CurrentLap, widget.TotalLaps));
    }

    /// <summary>
    /// Enables or disables dragging for this window.
    /// </summary>
    public void SetDraggingEnabled(bool enabled)
    {
        _isDraggingEnabled = enabled;

        if (enabled)
        {
            // Show drag overlay on top, block hit-testing on widget content
            DragOverlay.Visibility = Visibility.Visible;
            DragOverlay.Background = new SolidColorBrush(Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF));
            WidgetContent.IsHitTestVisible = false;

            DragOverlay.MouseLeftButtonDown += DragOverlay_MouseLeftButtonDown;
            DragOverlay.MouseMove += DragOverlay_MouseMove;
            DragOverlay.MouseLeftButtonUp += DragOverlay_MouseLeftButtonUp;
            DragOverlay.Cursor = System.Windows.Input.Cursors.SizeAll;
        }
        else
        {
            // Hide drag overlay, restore hit-testing on widget content
            DragOverlay.Visibility = Visibility.Collapsed;
            DragOverlay.Background = Brushes.Transparent;
            WidgetContent.IsHitTestVisible = true;

            DragOverlay.MouseLeftButtonDown -= DragOverlay_MouseLeftButtonDown;
            DragOverlay.MouseMove -= DragOverlay_MouseMove;
            DragOverlay.MouseLeftButtonUp -= DragOverlay_MouseLeftButtonUp;
            DragOverlay.Cursor = System.Windows.Input.Cursors.Arrow;
            _isDragging = false;
        }
    }

    private void DragOverlay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_isDraggingEnabled)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(null);
            DragOverlay.CaptureMouse();
        }
    }

    private void DragOverlay_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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

    private void DragOverlay_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            DragOverlay.ReleaseMouseCapture();
        }
    }

    private void WidgetOverlayWindow_LocationChanged(object? sender, EventArgs e)
    {
        ViewModel?.SaveWidgetPosition(Widget?.WidgetId, Left, Top);
    }

    protected override void OnClosed(EventArgs e)
    {
        // Unsubscribe from data updates
        if (_relativeOverlay != null)
        {
            _relativeOverlay.DataUpdated -= OnRelativeDataUpdated;
        }

        if (_fuelCalculator != null)
        {
            _fuelCalculator.DataUpdated -= OnFuelDataUpdated;
        }

        if (_inputsWidget != null)
        {
            _inputsWidget.DataUpdated -= OnInputsDataUpdated;
        }

        if (_inputTraceWidget != null)
        {
            _inputTraceWidget.DataUpdated -= OnInputTraceDataUpdated;
        }

        if (_standingsWidget != null)
        {
            _standingsWidget.DataUpdated -= OnStandingsDataUpdated;
        }

        // Unregister from drag service
        WidgetDragService.Instance.UnregisterWindow(this);
        base.OnClosed(e);
    }
}
