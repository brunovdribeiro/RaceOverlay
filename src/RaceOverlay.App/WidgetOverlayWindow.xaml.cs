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
    private LapTimerWidget? _lapTimerWidget;
    private LapTimerViewModel? _lapTimerViewModel;
    private TrackMapWidget? _trackMapWidget;
    private TrackMapViewModel? _trackMapViewModel;
    private WeatherWidget? _weatherWidget;
    private WeatherViewModel? _weatherViewModel;

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
        else if (Widget is LapTimerWidget lapTimerWidget)
        {
            _lapTimerWidget = lapTimerWidget;
            var view = new LapTimerView();
            _lapTimerViewModel = new LapTimerViewModel();

            if (lapTimerWidget.Configuration is ILapTimerConfig config)
            {
                _lapTimerViewModel.ApplyConfiguration(config);
            }

            _lapTimerViewModel.UpdateLapData(lapTimerWidget.GetLapTimerData());
            view.DataContext = _lapTimerViewModel;
            WidgetContent.Content = view;

            lapTimerWidget.DataUpdated += OnLapTimerDataUpdated;
        }
        else if (Widget is TrackMapWidget trackMapWidget)
        {
            _trackMapWidget = trackMapWidget;
            var view = new TrackMapView();
            _trackMapViewModel = new TrackMapViewModel();

            if (trackMapWidget.Configuration is ITrackMapConfig config)
            {
                _trackMapViewModel.ApplyConfiguration(config);
            }

            _trackMapViewModel.TrackOutline = trackMapWidget.GetTrackOutline();
            var data = trackMapWidget.GetTrackMapData();
            _trackMapViewModel.UpdateMap(data.Drivers, data.CurrentLap, data.TotalLaps);
            view.DataContext = _trackMapViewModel;
            WidgetContent.Content = view;

            trackMapWidget.DataUpdated += OnTrackMapDataUpdated;
        }
        else if (Widget is WeatherWidget weatherWidget)
        {
            _weatherWidget = weatherWidget;
            var view = new WeatherView();
            _weatherViewModel = new WeatherViewModel();

            if (weatherWidget.Configuration is IWeatherConfig config)
            {
                _weatherViewModel.ApplyConfiguration(config);
            }

            _weatherViewModel.UpdateWeather(weatherWidget.GetWeatherData());
            view.DataContext = _weatherViewModel;
            WidgetContent.Content = view;

            weatherWidget.DataUpdated += OnWeatherDataUpdated;
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

    public void ApplyLapTimerConfig(ILapTimerConfig config)
    {
        _lapTimerViewModel?.ApplyConfiguration(config);
    }

    public void ApplyTrackMapConfig(ITrackMapConfig config)
    {
        _trackMapViewModel?.ApplyConfiguration(config);
    }

    public void ApplyWeatherConfig(IWeatherConfig config)
    {
        _weatherViewModel?.ApplyConfiguration(config);
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

    private void OnLapTimerDataUpdated()
    {

        if (_lapTimerWidget == null || _lapTimerViewModel == null) return;

        var widget = _lapTimerWidget;
        var vm = _lapTimerViewModel;
        Dispatcher.Invoke(() => vm.UpdateLapData(widget.GetLapTimerData()));
    }

    private void OnTrackMapDataUpdated()
    {

        if (_trackMapWidget == null || _trackMapViewModel == null) return;

        var widget = _trackMapWidget;
        var vm = _trackMapViewModel;
        var data = widget.GetTrackMapData();
        Dispatcher.Invoke(() => vm.UpdateMap(data.Drivers, data.CurrentLap, data.TotalLaps));
    }

    private void OnWeatherDataUpdated()
    {

        if (_weatherWidget == null || _weatherViewModel == null) return;

        var widget = _weatherWidget;
        var vm = _weatherViewModel;
        Dispatcher.Invoke(() => vm.UpdateWeather(widget.GetWeatherData()));
    }

    /// <summary>
    /// Enables or disables dragging for this window.
    /// </summary>
    public void SetDraggingEnabled(bool enabled)
    {
        _isDraggingEnabled = enabled;

        if (enabled)
        {
            // Unsubscribe from data updates to eliminate all background work
            UnsubscribeDataUpdated();

            // Hide widget content but keep layout so window retains its size/boundary
            WidgetContent.Visibility = Visibility.Hidden;

            // Show drag overlay with "Drag Mode" message
            DragOverlay.Visibility = Visibility.Visible;
            DragOverlay.Background = new SolidColorBrush(Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF));

            DragOverlay.MouseLeftButtonDown += DragOverlay_MouseLeftButtonDown;
            DragOverlay.Cursor = System.Windows.Input.Cursors.SizeAll;
        }
        else
        {
            // Hide drag overlay, restore widget content
            DragOverlay.Visibility = Visibility.Collapsed;
            DragOverlay.Background = Brushes.Transparent;
            WidgetContent.Visibility = Visibility.Visible;

            DragOverlay.MouseLeftButtonDown -= DragOverlay_MouseLeftButtonDown;
            DragOverlay.Cursor = System.Windows.Input.Cursors.Arrow;

            // Refresh data once, then resubscribe for live updates
            RefreshWidgetData();
            SubscribeDataUpdated();
        }
    }

    private void RefreshWidgetData()
    {
        if (_relativeOverlay != null && _viewModel != null)
            _viewModel.RefreshDrivers(_relativeOverlay.GetRelativeDrivers());
        else if (_fuelCalculator != null && _fuelCalcViewModel != null)
            _fuelCalcViewModel.UpdateFuelData(_fuelCalculator.GetFuelData());
        else if (_inputsWidget != null && _inputsViewModel != null)
            _inputsViewModel.UpdateInputsData(_inputsWidget.GetInputsData());
        else if (_inputTraceWidget != null && _inputTraceViewModel != null)
            _inputTraceViewModel.UpdateTrace(_inputTraceWidget.GetTraceHistory());
        else if (_standingsWidget != null && _standingsViewModel != null)
            _standingsViewModel.UpdateStandings(_standingsWidget.GetStandings(), _standingsWidget.CurrentLap, _standingsWidget.TotalLaps);
        else if (_lapTimerWidget != null && _lapTimerViewModel != null)
            _lapTimerViewModel.UpdateLapData(_lapTimerWidget.GetLapTimerData());
        else if (_trackMapWidget != null && _trackMapViewModel != null)
        {
            _trackMapViewModel.TrackOutline = _trackMapWidget.GetTrackOutline();
            var data = _trackMapWidget.GetTrackMapData();
            _trackMapViewModel.UpdateMap(data.Drivers, data.CurrentLap, data.TotalLaps);
        }
        else if (_weatherWidget != null && _weatherViewModel != null)
            _weatherViewModel.UpdateWeather(_weatherWidget.GetWeatherData());
    }

    private void SubscribeDataUpdated()
    {
        if (_relativeOverlay != null) _relativeOverlay.DataUpdated += OnRelativeDataUpdated;
        if (_fuelCalculator != null) _fuelCalculator.DataUpdated += OnFuelDataUpdated;
        if (_inputsWidget != null) _inputsWidget.DataUpdated += OnInputsDataUpdated;
        if (_inputTraceWidget != null) _inputTraceWidget.DataUpdated += OnInputTraceDataUpdated;
        if (_standingsWidget != null) _standingsWidget.DataUpdated += OnStandingsDataUpdated;
        if (_lapTimerWidget != null) _lapTimerWidget.DataUpdated += OnLapTimerDataUpdated;
        if (_trackMapWidget != null) _trackMapWidget.DataUpdated += OnTrackMapDataUpdated;
        if (_weatherWidget != null) _weatherWidget.DataUpdated += OnWeatherDataUpdated;
    }

    private void UnsubscribeDataUpdated()
    {
        if (_relativeOverlay != null) _relativeOverlay.DataUpdated -= OnRelativeDataUpdated;
        if (_fuelCalculator != null) _fuelCalculator.DataUpdated -= OnFuelDataUpdated;
        if (_inputsWidget != null) _inputsWidget.DataUpdated -= OnInputsDataUpdated;
        if (_inputTraceWidget != null) _inputTraceWidget.DataUpdated -= OnInputTraceDataUpdated;
        if (_standingsWidget != null) _standingsWidget.DataUpdated -= OnStandingsDataUpdated;
        if (_lapTimerWidget != null) _lapTimerWidget.DataUpdated -= OnLapTimerDataUpdated;
        if (_trackMapWidget != null) _trackMapWidget.DataUpdated -= OnTrackMapDataUpdated;
        if (_weatherWidget != null) _weatherWidget.DataUpdated -= OnWeatherDataUpdated;
    }

    private void DragOverlay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_isDraggingEnabled)
        {
            DragMove();
        }
    }

    private void WidgetOverlayWindow_LocationChanged(object? sender, EventArgs e)
    {
        ViewModel?.SaveWidgetPosition(Widget?.WidgetId, Left, Top);
    }

    protected override void OnClosed(EventArgs e)
    {
        UnsubscribeDataUpdated();

        // Unregister from drag service
        WidgetDragService.Instance.UnregisterWindow(this);
        base.OnClosed(e);
    }
}
