using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RaceOverlay.App.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Widgets;
using Microsoft.Extensions.DependencyInjection;

namespace RaceOverlay.App.ViewModels;

/// <summary>
/// ViewModel for the main window that manages widget selection and configuration.
/// Uses MVVM Toolkit for observable properties and relay commands.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IWidgetRegistry _widgetRegistry;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, IWidget> _activeWidgets = new();
    private readonly Dictionary<string, WidgetOverlayWindow> _activeWindows = new();
    private readonly Dictionary<string, IWidgetConfiguration> _savedConfigs = new();

    [ObservableProperty]
    private WidgetMetadata? selectedWidget;

    [ObservableProperty]
    private ObservableCollection<WidgetMetadata> availableWidgets = new();

    [ObservableProperty]
    private bool isWidgetSelected;

    [ObservableProperty]
    private string selectedWidgetId = "";

    // Column visibility toggles (Relative Overlay)
    [ObservableProperty]
    private bool showPosition = true;

    [ObservableProperty]
    private bool showClassColor = true;

    [ObservableProperty]
    private bool showDriverName = true;

    [ObservableProperty]
    private bool showRating = true;

    [ObservableProperty]
    private bool showStint = true;

    [ObservableProperty]
    private bool showLapTime = true;

    [ObservableProperty]
    private bool showGap = true;

    [ObservableProperty]
    private string overlayPositionText = "Not set";

    // Data settings (Relative Overlay)
    [ObservableProperty]
    private int driversAhead = 3;

    [ObservableProperty]
    private int driversBehind = 3;

    [ObservableProperty]
    private int updateIntervalMs = 500;

    // Fuel Calculator settings
    [ObservableProperty]
    private double fuelTankCapacity = 110.0;

    // Inputs settings
    [ObservableProperty]
    private int inputsUpdateIntervalMs = 16;

    [ObservableProperty]
    private string inputsThrottleColor = "#22C55E";

    [ObservableProperty]
    private string inputsBrakeColor = "#EF4444";

    [ObservableProperty]
    private string inputsClutchColor = "#3B82F6";

    [ObservableProperty]
    private bool inputsShowClutch = false;

    // Input Trace settings
    [ObservableProperty]
    private int inputTraceUpdateIntervalMs = 16;

    [ObservableProperty]
    private string inputTraceThrottleColor = "#22C55E";

    [ObservableProperty]
    private string inputTraceBrakeColor = "#EF4444";

    [ObservableProperty]
    private int inputTraceHistorySeconds = 10;

    /// <summary>
    /// Initializes a new instance of the MainWindowViewModel.
    /// </summary>
    public MainWindowViewModel(IWidgetRegistry widgetRegistry, IServiceProvider serviceProvider)
    {
        _widgetRegistry = widgetRegistry ?? throw new ArgumentNullException(nameof(widgetRegistry));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        // Load available widgets from the registry
        LoadAvailableWidgets();
    }

    /// <summary>
    /// Called when selected widget changes to load its configuration.
    /// </summary>
    partial void OnSelectedWidgetChanged(WidgetMetadata? value)
    {
        if (value == null)
        {
            IsWidgetSelected = false;
            SelectedWidgetId = "";
            return;
        }

        IsWidgetSelected = true;
        SelectedWidgetId = value.WidgetId;

        if (value.WidgetId == "fuel-calculator")
        {
            if (_savedConfigs.TryGetValue(value.WidgetId, out var saved) && saved is IFuelCalculatorConfig fuelConfig)
            {
                LoadConfigFromFuelWidget(fuelConfig);
            }
            else
            {
                var activeInstance = _activeWidgets.Values
                    .FirstOrDefault(w => w.WidgetId == value.WidgetId);

                if (activeInstance?.Configuration is IFuelCalculatorConfig config)
                {
                    LoadConfigFromFuelWidget(config);
                }
                else
                {
                    LoadConfigFromFuelWidget(new FuelCalculatorConfig());
                }
            }
        }
        else if (value.WidgetId == "inputs")
        {
            if (_savedConfigs.TryGetValue(value.WidgetId, out var saved) && saved is IInputsConfig inputsConfig)
            {
                LoadConfigFromInputsWidget(inputsConfig);
            }
            else
            {
                var activeInstance = _activeWidgets.Values
                    .FirstOrDefault(w => w.WidgetId == value.WidgetId);

                if (activeInstance?.Configuration is IInputsConfig config)
                {
                    LoadConfigFromInputsWidget(config);
                }
                else
                {
                    LoadConfigFromInputsWidget(new InputsConfig());
                }
            }
        }
        else if (value.WidgetId == "input-trace")
        {
            if (_savedConfigs.TryGetValue(value.WidgetId, out var saved) && saved is IInputTraceConfig inputTraceConfig)
            {
                LoadConfigFromInputTraceWidget(inputTraceConfig);
            }
            else
            {
                var activeInstance = _activeWidgets.Values
                    .FirstOrDefault(w => w.WidgetId == value.WidgetId);

                if (activeInstance?.Configuration is IInputTraceConfig config)
                {
                    LoadConfigFromInputTraceWidget(config);
                }
                else
                {
                    LoadConfigFromInputTraceWidget(new InputTraceConfig());
                }
            }
        }
        else
        {
            // Relative Overlay or other widgets
            if (_savedConfigs.TryGetValue(value.WidgetId, out var saved) && saved is IRelativeOverlayConfig relConfig)
            {
                LoadConfigFromRelativeWidget(relConfig);
            }
            else
            {
                var activeInstance = _activeWidgets.Values
                    .FirstOrDefault(w => w.WidgetId == value.WidgetId);

                if (activeInstance?.Configuration is IRelativeOverlayConfig config)
                {
                    LoadConfigFromRelativeWidget(config);
                }
                else
                {
                    LoadConfigFromRelativeWidget(new RelativeOverlayConfig());
                }
            }
        }
    }

    private void LoadConfigFromRelativeWidget(IRelativeOverlayConfig config)
    {
        ShowPosition = config.ShowPosition;
        ShowClassColor = config.ShowClassColor;
        ShowDriverName = config.ShowDriverName;
        ShowRating = config.ShowRating;
        ShowStint = config.ShowStint;
        ShowLapTime = config.ShowLapTime;
        ShowGap = config.ShowGap;
        DriversAhead = config.DriversAhead;
        DriversBehind = config.DriversBehind;
        UpdateIntervalMs = config.UpdateIntervalMs;
        UpdatePositionText(config.OverlayLeft, config.OverlayTop);
    }

    private void LoadConfigFromFuelWidget(IFuelCalculatorConfig config)
    {
        FuelTankCapacity = config.FuelTankCapacity;
        UpdatePositionText(config.OverlayLeft, config.OverlayTop);
    }

    private void LoadConfigFromInputsWidget(IInputsConfig config)
    {
        InputsUpdateIntervalMs = config.UpdateIntervalMs;
        InputsThrottleColor = config.ThrottleColor;
        InputsBrakeColor = config.BrakeColor;
        InputsClutchColor = config.ClutchColor;
        InputsShowClutch = config.ShowClutch;
        UpdatePositionText(config.OverlayLeft, config.OverlayTop);
    }

    private void LoadConfigFromInputTraceWidget(IInputTraceConfig config)
    {
        InputTraceUpdateIntervalMs = config.UpdateIntervalMs;
        InputTraceThrottleColor = config.ThrottleColor;
        InputTraceBrakeColor = config.BrakeColor;
        InputTraceHistorySeconds = config.HistorySeconds;
        UpdatePositionText(config.OverlayLeft, config.OverlayTop);
    }

    // Push config changes to active widget instances when toggles change
    partial void OnShowPositionChanged(bool value) => PushRelativeConfigToActiveWidgets();
    partial void OnShowClassColorChanged(bool value) => PushRelativeConfigToActiveWidgets();
    partial void OnShowDriverNameChanged(bool value) => PushRelativeConfigToActiveWidgets();
    partial void OnShowRatingChanged(bool value) => PushRelativeConfigToActiveWidgets();
    partial void OnShowStintChanged(bool value) => PushRelativeConfigToActiveWidgets();
    partial void OnShowLapTimeChanged(bool value) => PushRelativeConfigToActiveWidgets();
    partial void OnShowGapChanged(bool value) => PushRelativeConfigToActiveWidgets();
    partial void OnDriversAheadChanged(int value) => PushRelativeConfigToActiveWidgets();
    partial void OnDriversBehindChanged(int value) => PushRelativeConfigToActiveWidgets();
    partial void OnUpdateIntervalMsChanged(int value) => PushRelativeConfigToActiveWidgets();

    partial void OnFuelTankCapacityChanged(double value) => PushFuelConfigToActiveWidgets();

    partial void OnInputsUpdateIntervalMsChanged(int value) => PushInputsConfigToActiveWidgets();
    partial void OnInputsThrottleColorChanged(string value) => PushInputsConfigToActiveWidgets();
    partial void OnInputsBrakeColorChanged(string value) => PushInputsConfigToActiveWidgets();
    partial void OnInputsClutchColorChanged(string value) => PushInputsConfigToActiveWidgets();
    partial void OnInputsShowClutchChanged(bool value) => PushInputsConfigToActiveWidgets();

    partial void OnInputTraceUpdateIntervalMsChanged(int value) => PushInputTraceConfigToActiveWidgets();
    partial void OnInputTraceThrottleColorChanged(string value) => PushInputTraceConfigToActiveWidgets();
    partial void OnInputTraceBrakeColorChanged(string value) => PushInputTraceConfigToActiveWidgets();
    partial void OnInputTraceHistorySecondsChanged(int value) => PushInputTraceConfigToActiveWidgets();

    private void PushRelativeConfigToActiveWidgets()
    {
        if (SelectedWidget == null || SelectedWidgetId != "relative-overlay") return;

        // Preserve saved position if it exists
        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedWidget.WidgetId, out var existing) && existing is RelativeOverlayConfig existingRel)
        {
            left = existingRel.OverlayLeft;
            top = existingRel.OverlayTop;
        }

        var config = new RelativeOverlayConfig
        {
            ShowPosition = ShowPosition,
            ShowClassColor = ShowClassColor,
            ShowDriverName = ShowDriverName,
            ShowRating = ShowRating,
            ShowStint = ShowStint,
            ShowLapTime = ShowLapTime,
            ShowGap = ShowGap,
            DriversAhead = DriversAhead,
            DriversBehind = DriversBehind,
            UpdateIntervalMs = UpdateIntervalMs,
            OverlayLeft = left,
            OverlayTop = top
        };

        _savedConfigs[SelectedWidget.WidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedWidget.WidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedWidget.WidgetId))
            {
                kv.Value.ApplyColumnVisibility(config);
            }
        }
    }

    private void PushInputsConfigToActiveWidgets()
    {
        if (SelectedWidget == null || SelectedWidgetId != "inputs") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedWidget.WidgetId, out var existing) && existing is InputsConfig existingInputs)
        {
            left = existingInputs.OverlayLeft;
            top = existingInputs.OverlayTop;
        }

        var config = new InputsConfig
        {
            UpdateIntervalMs = InputsUpdateIntervalMs,
            ThrottleColor = InputsThrottleColor,
            BrakeColor = InputsBrakeColor,
            ClutchColor = InputsClutchColor,
            ShowClutch = InputsShowClutch,
            OverlayLeft = left,
            OverlayTop = top
        };

        _savedConfigs[SelectedWidget.WidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedWidget.WidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedWidget.WidgetId))
            {
                kv.Value.ApplyInputsConfig(config);
            }
        }
    }

    private void PushInputTraceConfigToActiveWidgets()
    {
        if (SelectedWidget == null || SelectedWidgetId != "input-trace") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedWidget.WidgetId, out var existing) && existing is InputTraceConfig existingTrace)
        {
            left = existingTrace.OverlayLeft;
            top = existingTrace.OverlayTop;
        }

        var config = new InputTraceConfig
        {
            UpdateIntervalMs = InputTraceUpdateIntervalMs,
            ThrottleColor = InputTraceThrottleColor,
            BrakeColor = InputTraceBrakeColor,
            HistorySeconds = InputTraceHistorySeconds,
            OverlayLeft = left,
            OverlayTop = top
        };

        _savedConfigs[SelectedWidget.WidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedWidget.WidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedWidget.WidgetId))
            {
                kv.Value.ApplyInputTraceConfig(config);
            }
        }
    }

    private void PushFuelConfigToActiveWidgets()
    {
        if (SelectedWidget == null || SelectedWidgetId != "fuel-calculator") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedWidget.WidgetId, out var existing) && existing is FuelCalculatorConfig existingFuel)
        {
            left = existingFuel.OverlayLeft;
            top = existingFuel.OverlayTop;
        }

        var config = new FuelCalculatorConfig
        {
            FuelTankCapacity = FuelTankCapacity,
            OverlayLeft = left,
            OverlayTop = top
        };

        _savedConfigs[SelectedWidget.WidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedWidget.WidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedWidget.WidgetId))
            {
                kv.Value.ApplyFuelConfig(config);
            }
        }
    }

    /// <summary>
    /// Saves the overlay window position for the given widget type.
    /// Called by WidgetOverlayWindow on LocationChanged.
    /// </summary>
    public void SaveWidgetPosition(string? widgetId, double left, double top)
    {
        if (widgetId == null) return;

        if (_savedConfigs.TryGetValue(widgetId, out var config))
        {
            if (config is RelativeOverlayConfig relConfig)
            {
                relConfig.OverlayLeft = left;
                relConfig.OverlayTop = top;
            }
            else if (config is FuelCalculatorConfig fuelConfig)
            {
                fuelConfig.OverlayLeft = left;
                fuelConfig.OverlayTop = top;
            }
            else if (config is InputsConfig inputsConfig)
            {
                inputsConfig.OverlayLeft = left;
                inputsConfig.OverlayTop = top;
            }
            else if (config is InputTraceConfig inputTraceConfig)
            {
                inputTraceConfig.OverlayLeft = left;
                inputTraceConfig.OverlayTop = top;
            }
        }
        else
        {
            if (widgetId == "fuel-calculator")
            {
                _savedConfigs[widgetId] = new FuelCalculatorConfig { OverlayLeft = left, OverlayTop = top };
            }
            else if (widgetId == "inputs")
            {
                _savedConfigs[widgetId] = new InputsConfig { OverlayLeft = left, OverlayTop = top };
            }
            else if (widgetId == "input-trace")
            {
                _savedConfigs[widgetId] = new InputTraceConfig { OverlayLeft = left, OverlayTop = top };
            }
            else
            {
                _savedConfigs[widgetId] = new RelativeOverlayConfig { OverlayLeft = left, OverlayTop = top };
            }
        }

        if (SelectedWidget?.WidgetId == widgetId)
        {
            UpdatePositionText(left, top);
        }
    }

    private void UpdatePositionText(double left, double top)
    {
        if (double.IsNaN(left) || double.IsNaN(top))
            OverlayPositionText = "Not set";
        else
            OverlayPositionText = $"{(int)left}, {(int)top}";
    }

    /// <summary>
    /// Loads all available widgets from the registry into the view.
    /// </summary>
    private void LoadAvailableWidgets()
    {
        AvailableWidgets.Clear();

        var registeredWidgets = _widgetRegistry.GetRegisteredWidgets();
        foreach (var widget in registeredWidgets)
        {
            AvailableWidgets.Add(widget);
        }
    }

    /// <summary>
    /// Command to add the selected widget to the application.
    /// </summary>
    [RelayCommand]
    private async Task AddWidget()
    {
        if (SelectedWidget == null)
        {
            return;
        }

        try
        {
            // Create widget instance
            var widgetInstance = _serviceProvider.GetService(SelectedWidget.WidgetType) as IWidget;
            if (widgetInstance == null)
            {
                return;
            }

            // Generate unique ID for this instance
            string instanceId = $"{SelectedWidget.WidgetId}-{Guid.NewGuid():N}";

            // Store in active widgets
            _activeWidgets[instanceId] = widgetInstance;

            // Start the widget
            await widgetInstance.StartAsync();

            // Show the overlay window
            ShowWidgetOverlay(widgetInstance, SelectedWidget.DisplayName, instanceId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding widget: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows the widget in an overlay window on top of other applications.
    /// </summary>
    private void ShowWidgetOverlay(IWidget widget, string displayName, string instanceId)
    {
        // Create the overlay window
        var overlayWindow = new WidgetOverlayWindow
        {
            Title = displayName,
            Widget = widget,
            InstanceId = instanceId,
            ViewModel = this
        };

        // Restore saved position if available
        if (_savedConfigs.TryGetValue(widget.WidgetId, out var savedConfig))
        {
            widget.UpdateConfiguration(savedConfig);

            double savedLeft = double.NaN, savedTop = double.NaN;
            if (savedConfig is RelativeOverlayConfig relConfig)
            {
                savedLeft = relConfig.OverlayLeft;
                savedTop = relConfig.OverlayTop;
            }
            else if (savedConfig is FuelCalculatorConfig fuelConfig)
            {
                savedLeft = fuelConfig.OverlayLeft;
                savedTop = fuelConfig.OverlayTop;
            }
            else if (savedConfig is InputsConfig inputsConfig)
            {
                savedLeft = inputsConfig.OverlayLeft;
                savedTop = inputsConfig.OverlayTop;
            }
            else if (savedConfig is InputTraceConfig inputTraceConfig)
            {
                savedLeft = inputTraceConfig.OverlayLeft;
                savedTop = inputTraceConfig.OverlayTop;
            }

            if (!double.IsNaN(savedLeft) && !double.IsNaN(savedTop))
            {
                overlayWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                overlayWindow.Left = savedLeft;
                overlayWindow.Top = savedTop;
            }
        }

        // Track the window
        _activeWindows[instanceId] = overlayWindow;

        // Handle window closing to clean up tracking
        overlayWindow.Closed += (s, e) =>
        {
            _activeWindows.Remove(instanceId);
        };

        // Show the window on top
        overlayWindow.Show();
    }

    /// <summary>
    /// Removes an active widget instance and closes its window.
    /// </summary>
    public async Task RemoveWidgetInstance(string instanceId)
    {
        // Close the overlay window if it exists
        if (_activeWindows.TryGetValue(instanceId, out var window))
        {
            window.Close();
            _activeWindows.Remove(instanceId);
        }

        // Stop and remove the widget
        if (_activeWidgets.TryGetValue(instanceId, out var widget))
        {
            await widget.StopAsync();
            _activeWidgets.Remove(instanceId);
        }
    }

    /// <summary>
    /// Command to remove the selected widget from the application.
    /// </summary>
    [RelayCommand]
    private async Task RemoveWidget()
    {
        if (SelectedWidget == null)
        {
            return;
        }

        // Remove all instances of this widget type
        var instancesToRemove = _activeWidgets
            .Where(kv => kv.Key.StartsWith(SelectedWidget.WidgetId))
            .Select(kv => kv.Key)
            .ToList();

        foreach (var instanceId in instancesToRemove)
        {
            await RemoveWidgetInstance(instanceId);
        }
    }

    /// <summary>
    /// Registers a new widget type with the registry.
    /// </summary>
    public void RegisterWidget(WidgetMetadata metadata)
    {
        _widgetRegistry.RegisterWidget(metadata);
        LoadAvailableWidgets();
    }

    /// <summary>
    /// Unregisters a widget type from the registry.
    /// </summary>
    public bool UnregisterWidget(string widgetId)
    {
        var result = _widgetRegistry.UnregisterWidget(widgetId);
        if (result)
        {
            LoadAvailableWidgets();
        }

        return result;
    }
}
