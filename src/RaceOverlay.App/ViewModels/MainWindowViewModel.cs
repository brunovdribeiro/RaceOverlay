using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RaceOverlay.App.Models;
using RaceOverlay.App.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Factories;
using RaceOverlay.Engine.Widgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RaceOverlay.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IWidgetRegistry _widgetRegistry;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly WidgetViewFactoryRegistry _factoryRegistry;
    private readonly ConfigurationPersistenceService _persistenceService = new();
    private readonly Dictionary<string, IWidget> _activeWidgets = new();
    private readonly Dictionary<string, WidgetHostPanel> _activeHostPanels = new();
    private readonly Dictionary<string, IWidgetConfiguration> _savedConfigs = new();
    private OverlayHostWindow? _overlayHost;
    private bool _isRestoring;

    private static readonly Dictionary<string, string> WidgetIconKeys = new()
    {
        ["relative-overlay"] = "Icon.Gauge",
        ["fuel-calculator"] = "Icon.Fuel",
        ["inputs"] = "Icon.Gamepad",
        ["input-trace"] = "Icon.Waveform",
        ["standings"] = "Icon.Podium",
        ["lap-timer"] = "Icon.Stopwatch",
        ["track-map"] = "Icon.MapPin",
        ["weather"] = "Icon.Cloud"
    };

    [ObservableProperty]
    private ObservableCollection<WidgetLibraryItem> widgetLibraryItems = new();

    [ObservableProperty]
    private ObservableCollection<ActiveWidgetCard> activeWidgetCards = new();

    [ObservableProperty]
    private ActiveWidgetCard? selectedActiveCard;

    [ObservableProperty]
    private bool isActiveWidgetSelected;

    [ObservableProperty]
    private string selectedConfigWidgetId = "";

    [ObservableProperty]
    private bool isSetupMode;

    [ObservableProperty]
    private string overlayPositionText = "Not set";

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

    // Standings settings
    [ObservableProperty]
    private int standingsUpdateIntervalMs = 500;

    [ObservableProperty]
    private bool standingsShowClassColor = true;

    [ObservableProperty]
    private bool standingsShowCarNumber = true;

    [ObservableProperty]
    private bool standingsShowPositionsGained = true;

    [ObservableProperty]
    private bool standingsShowLicense = true;

    [ObservableProperty]
    private bool standingsShowIRating = true;

    [ObservableProperty]
    private bool standingsShowCarBrand = true;

    [ObservableProperty]
    private bool standingsShowInterval = true;

    [ObservableProperty]
    private bool standingsShowGap = true;

    [ObservableProperty]
    private bool standingsShowLastLapTime = true;

    [ObservableProperty]
    private bool standingsShowDelta = true;

    [ObservableProperty]
    private bool standingsShowPitStatus = true;

    [ObservableProperty]
    private int standingsMaxDrivers = 20;

    // Lap Timer settings
    [ObservableProperty]
    private int lapTimerUpdateIntervalMs = 50;

    [ObservableProperty]
    private bool lapTimerShowDeltaToBest = true;

    [ObservableProperty]
    private bool lapTimerShowLastLap = true;

    [ObservableProperty]
    private bool lapTimerShowBestLap = true;

    [ObservableProperty]
    private bool lapTimerShowDeltaLastBest = true;

    // Track Map settings
    [ObservableProperty]
    private int trackMapUpdateIntervalMs = 50;

    [ObservableProperty]
    private bool trackMapShowDriverNames = false;

    [ObservableProperty]
    private bool trackMapShowPitStatus = true;

    // Weather settings
    [ObservableProperty]
    private int weatherUpdateIntervalMs = 2000;

    [ObservableProperty]
    private bool weatherShowWind = true;

    [ObservableProperty]
    private bool weatherShowForecast = true;

    // Input Trace settings
    [ObservableProperty]
    private int inputTraceUpdateIntervalMs = 16;

    [ObservableProperty]
    private string inputTraceThrottleColor = "#22C55E";

    [ObservableProperty]
    private string inputTraceBrakeColor = "#EF4444";

    [ObservableProperty]
    private int inputTraceHistorySeconds = 10;

    public string SetupModeButtonText => IsSetupMode ? "Exit Setup Mode (Ctrl+F1)" : "Enter Setup Mode (Ctrl+F1)";

    public MainWindowViewModel(IWidgetRegistry widgetRegistry, IServiceProvider serviceProvider, ILogger<MainWindowViewModel> logger, WidgetViewFactoryRegistry factoryRegistry)
    {
        _widgetRegistry = widgetRegistry ?? throw new ArgumentNullException(nameof(widgetRegistry));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _factoryRegistry = factoryRegistry ?? throw new ArgumentNullException(nameof(factoryRegistry));

        LoadWidgetLibrary();
    }

    partial void OnIsSetupModeChanged(bool value)
    {
        OnPropertyChanged(nameof(SetupModeButtonText));
    }

    partial void OnSelectedActiveCardChanged(ActiveWidgetCard? oldValue, ActiveWidgetCard? newValue)
    {
        if (oldValue != null)
            oldValue.IsSelected = false;

        if (newValue == null)
        {
            IsActiveWidgetSelected = false;
            SelectedConfigWidgetId = "";
            return;
        }

        newValue.IsSelected = true;
        IsActiveWidgetSelected = true;
        SelectedConfigWidgetId = newValue.WidgetId;

        LoadConfigForWidget(newValue.WidgetId);
    }

    private static readonly Dictionary<string, (Func<IWidgetConfiguration, bool> IsMatch, Func<IWidgetConfiguration> CreateDefault)> ConfigTypeMap = new()
    {
        ["relative-overlay"] = (c => c is IRelativeOverlayConfig, () => new RelativeOverlayConfig()),
        ["fuel-calculator"] = (c => c is IFuelCalculatorConfig, () => new FuelCalculatorConfig()),
        ["inputs"] = (c => c is IInputsConfig, () => new InputsConfig()),
        ["input-trace"] = (c => c is IInputTraceConfig, () => new InputTraceConfig()),
        ["standings"] = (c => c is IStandingsConfig, () => new StandingsConfig()),
        ["lap-timer"] = (c => c is ILapTimerConfig, () => new LapTimerConfig()),
        ["track-map"] = (c => c is ITrackMapConfig, () => new TrackMapConfig()),
        ["weather"] = (c => c is IWeatherConfig, () => new WeatherConfig()),
    };

    private void LoadConfigForWidget(string widgetId)
    {
        var config = ResolveConfig(widgetId);
        LoadConfigIntoUI(widgetId, config);
    }

    private IWidgetConfiguration ResolveConfig(string widgetId)
    {
        if (!ConfigTypeMap.TryGetValue(widgetId, out var entry))
            return new RelativeOverlayConfig();

        if (_savedConfigs.TryGetValue(widgetId, out var saved) && entry.IsMatch(saved))
            return saved;

        var instance = _activeWidgets.Values.FirstOrDefault(w => w.WidgetId == widgetId);
        if (instance?.Configuration is { } cfg && entry.IsMatch(cfg))
            return cfg;

        return entry.CreateDefault();
    }

    private void LoadConfigIntoUI(string widgetId, IWidgetConfiguration config)
    {
        switch (widgetId)
        {
            case "relative-overlay": LoadConfigFromRelativeWidget((IRelativeOverlayConfig)config); break;
            case "fuel-calculator": LoadConfigFromFuelWidget((IFuelCalculatorConfig)config); break;
            case "inputs": LoadConfigFromInputsWidget((IInputsConfig)config); break;
            case "input-trace": LoadConfigFromInputTraceWidget((IInputTraceConfig)config); break;
            case "standings": LoadConfigFromStandingsWidget((IStandingsConfig)config); break;
            case "lap-timer": LoadConfigFromLapTimerWidget((ILapTimerConfig)config); break;
            case "track-map": LoadConfigFromTrackMapWidget((ITrackMapConfig)config); break;
            case "weather": LoadConfigFromWeatherWidget((IWeatherConfig)config); break;
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

    private void LoadConfigFromStandingsWidget(IStandingsConfig config)
    {
        StandingsUpdateIntervalMs = config.UpdateIntervalMs;
        StandingsShowClassColor = config.ShowClassColor;
        StandingsShowCarNumber = config.ShowCarNumber;
        StandingsShowPositionsGained = config.ShowPositionsGained;
        StandingsShowLicense = config.ShowLicense;
        StandingsShowIRating = config.ShowIRating;
        StandingsShowCarBrand = config.ShowCarBrand;
        StandingsShowInterval = config.ShowInterval;
        StandingsShowGap = config.ShowGap;
        StandingsShowLastLapTime = config.ShowLastLapTime;
        StandingsShowDelta = config.ShowDelta;
        StandingsShowPitStatus = config.ShowPitStatus;
        StandingsMaxDrivers = config.MaxDrivers;
        UpdatePositionText(config.OverlayLeft, config.OverlayTop);
    }

    private void LoadConfigFromLapTimerWidget(ILapTimerConfig config)
    {
        LapTimerUpdateIntervalMs = config.UpdateIntervalMs;
        LapTimerShowDeltaToBest = config.ShowDeltaToBest;
        LapTimerShowLastLap = config.ShowLastLap;
        LapTimerShowBestLap = config.ShowBestLap;
        LapTimerShowDeltaLastBest = config.ShowDeltaLastBest;
        UpdatePositionText(config.OverlayLeft, config.OverlayTop);
    }

    private void LoadConfigFromTrackMapWidget(ITrackMapConfig config)
    {
        TrackMapUpdateIntervalMs = config.UpdateIntervalMs;
        TrackMapShowDriverNames = config.ShowDriverNames;
        TrackMapShowPitStatus = config.ShowPitStatus;
        UpdatePositionText(config.OverlayLeft, config.OverlayTop);
    }

    private void LoadConfigFromWeatherWidget(IWeatherConfig config)
    {
        WeatherUpdateIntervalMs = config.UpdateIntervalMs;
        WeatherShowWind = config.ShowWind;
        WeatherShowForecast = config.ShowForecast;
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

    partial void OnStandingsUpdateIntervalMsChanged(int value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowClassColorChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowCarNumberChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowPositionsGainedChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowLicenseChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowIRatingChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowCarBrandChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowIntervalChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowGapChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowLastLapTimeChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowDeltaChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsShowPitStatusChanged(bool value) => PushStandingsConfigToActiveWidgets();
    partial void OnStandingsMaxDriversChanged(int value) => PushStandingsConfigToActiveWidgets();

    partial void OnLapTimerUpdateIntervalMsChanged(int value) => PushLapTimerConfigToActiveWidgets();
    partial void OnLapTimerShowDeltaToBestChanged(bool value) => PushLapTimerConfigToActiveWidgets();
    partial void OnLapTimerShowLastLapChanged(bool value) => PushLapTimerConfigToActiveWidgets();
    partial void OnLapTimerShowBestLapChanged(bool value) => PushLapTimerConfigToActiveWidgets();
    partial void OnLapTimerShowDeltaLastBestChanged(bool value) => PushLapTimerConfigToActiveWidgets();

    partial void OnTrackMapUpdateIntervalMsChanged(int value) => PushTrackMapConfigToActiveWidgets();
    partial void OnTrackMapShowDriverNamesChanged(bool value) => PushTrackMapConfigToActiveWidgets();
    partial void OnTrackMapShowPitStatusChanged(bool value) => PushTrackMapConfigToActiveWidgets();

    partial void OnWeatherUpdateIntervalMsChanged(int value) => PushWeatherConfigToActiveWidgets();
    partial void OnWeatherShowWindChanged(bool value) => PushWeatherConfigToActiveWidgets();
    partial void OnWeatherShowForecastChanged(bool value) => PushWeatherConfigToActiveWidgets();

    private void PushConfigToActiveWidgets(string widgetId, IWidgetConfiguration config)
    {
        if (SelectedConfigWidgetId != widgetId) return;

        // Preserve overlay position from any previously saved config
        if (_savedConfigs.TryGetValue(widgetId, out var existing))
        {
            config.OverlayLeft = existing.OverlayLeft;
            config.OverlayTop = existing.OverlayTop;
        }

        _savedConfigs[widgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(widgetId))
                kv.Value.UpdateConfiguration(config);
        }

        foreach (var kv in _activeHostPanels)
        {
            if (kv.Key.StartsWith(widgetId))
                kv.Value.ApplyConfig(config);
        }
    }

    private void PushRelativeConfigToActiveWidgets() => PushConfigToActiveWidgets("relative-overlay", new RelativeOverlayConfig
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
    });

    private void PushFuelConfigToActiveWidgets() => PushConfigToActiveWidgets("fuel-calculator", new FuelCalculatorConfig
    {
        FuelTankCapacity = FuelTankCapacity,
    });

    private void PushInputsConfigToActiveWidgets() => PushConfigToActiveWidgets("inputs", new InputsConfig
    {
        UpdateIntervalMs = InputsUpdateIntervalMs,
        ThrottleColor = InputsThrottleColor,
        BrakeColor = InputsBrakeColor,
        ClutchColor = InputsClutchColor,
        ShowClutch = InputsShowClutch,
    });

    private void PushInputTraceConfigToActiveWidgets() => PushConfigToActiveWidgets("input-trace", new InputTraceConfig
    {
        UpdateIntervalMs = InputTraceUpdateIntervalMs,
        ThrottleColor = InputTraceThrottleColor,
        BrakeColor = InputTraceBrakeColor,
        HistorySeconds = InputTraceHistorySeconds,
    });

    private void PushStandingsConfigToActiveWidgets() => PushConfigToActiveWidgets("standings", new StandingsConfig
    {
        UpdateIntervalMs = StandingsUpdateIntervalMs,
        ShowClassColor = StandingsShowClassColor,
        ShowCarNumber = StandingsShowCarNumber,
        ShowPositionsGained = StandingsShowPositionsGained,
        ShowLicense = StandingsShowLicense,
        ShowIRating = StandingsShowIRating,
        ShowCarBrand = StandingsShowCarBrand,
        ShowInterval = StandingsShowInterval,
        ShowGap = StandingsShowGap,
        ShowLastLapTime = StandingsShowLastLapTime,
        ShowDelta = StandingsShowDelta,
        ShowPitStatus = StandingsShowPitStatus,
        MaxDrivers = StandingsMaxDrivers,
    });

    private void PushLapTimerConfigToActiveWidgets() => PushConfigToActiveWidgets("lap-timer", new LapTimerConfig
    {
        UpdateIntervalMs = LapTimerUpdateIntervalMs,
        ShowDeltaToBest = LapTimerShowDeltaToBest,
        ShowLastLap = LapTimerShowLastLap,
        ShowBestLap = LapTimerShowBestLap,
        ShowDeltaLastBest = LapTimerShowDeltaLastBest,
    });

    private void PushTrackMapConfigToActiveWidgets() => PushConfigToActiveWidgets("track-map", new TrackMapConfig
    {
        UpdateIntervalMs = TrackMapUpdateIntervalMs,
        ShowDriverNames = TrackMapShowDriverNames,
        ShowPitStatus = TrackMapShowPitStatus,
    });

    private void PushWeatherConfigToActiveWidgets() => PushConfigToActiveWidgets("weather", new WeatherConfig
    {
        UpdateIntervalMs = WeatherUpdateIntervalMs,
        ShowWind = WeatherShowWind,
        ShowForecast = WeatherShowForecast,
    });

    public void SaveWidgetPosition(string? widgetId, double left, double top)
    {
        if (widgetId == null) return;

        if (_savedConfigs.TryGetValue(widgetId, out var config))
        {
            config.OverlayLeft = left;
            config.OverlayTop = top;
        }

        if (SelectedActiveCard?.WidgetId == widgetId)
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

    private void LoadWidgetLibrary()
    {
        WidgetLibraryItems.Clear();

        var registeredWidgets = _widgetRegistry.GetRegisteredWidgets();
        foreach (var widget in registeredWidgets)
        {
            WidgetLibraryItems.Add(new WidgetLibraryItem(widget, OnWidgetToggled));
        }
    }

    private async void OnWidgetToggled(WidgetLibraryItem item, bool isEnabled)
    {
        if (_isRestoring) return;

        if (isEnabled)
            await ActivateWidget(item.Metadata);
        else
            await DeactivateWidget(item.Metadata.WidgetId);
    }

    private async Task ActivateWidget(WidgetMetadata metadata)
    {
        try
        {
            var widgetInstance = _serviceProvider.GetService(metadata.WidgetType) as IWidget;
            if (widgetInstance == null) return;

            string instanceId = $"{metadata.WidgetId}-{Guid.NewGuid():N}";
            _activeWidgets[instanceId] = widgetInstance;

            await widgetInstance.StartAsync();
            ShowWidgetOverlay(widgetInstance, metadata.DisplayName, instanceId);

            // Create active widget card
            var iconKey = WidgetIconKeys.GetValueOrDefault(metadata.WidgetId, "Icon.Gauge");
            var card = new ActiveWidgetCard(metadata.WidgetId, metadata.DisplayName, iconKey);
            ActiveWidgetCards.Add(card);

            SaveConfiguration();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding widget");
        }
    }

    private async Task DeactivateWidget(string widgetId)
    {
        var instancesToRemove = _activeWidgets
            .Where(kv => kv.Key.StartsWith(widgetId))
            .Select(kv => kv.Key)
            .ToList();

        foreach (var instanceId in instancesToRemove)
        {
            await RemoveWidgetInstance(instanceId);
        }

        // Remove the card
        var card = ActiveWidgetCards.FirstOrDefault(c => c.WidgetId == widgetId);
        if (card != null)
        {
            if (SelectedActiveCard == card)
                SelectedActiveCard = null;
            ActiveWidgetCards.Remove(card);
        }

        SaveConfiguration();
    }

    [RelayCommand]
    private void SelectActiveCard(ActiveWidgetCard? card)
    {
        SelectedActiveCard = card;
    }

    [RelayCommand]
    private void ToggleSetupMode()
    {
        WidgetDragService.Instance.ToggleDragging();
        IsSetupMode = WidgetDragService.Instance.IsDraggingEnabled;

        if (!IsSetupMode)
        {
            SaveConfiguration();
        }
    }

    [RelayCommand]
    private void StartLayout()
    {
        SaveConfiguration();
        if (Application.Current.MainWindow != null)
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
    }

    private void EnsureOverlayHost()
    {
        if (_overlayHost == null || !_overlayHost.IsLoaded)
        {
            _overlayHost = new OverlayHostWindow();
            _overlayHost.Show();
        }
    }

    private void ShowWidgetOverlay(IWidget widget, string displayName, string instanceId)
    {
        EnsureOverlayHost();

        var panel = new WidgetHostPanel
        {
            Widget = widget,
            InstanceId = instanceId,
            ViewModel = this,
            FactoryRegistry = _factoryRegistry
        };

        // Set AutomationProperties.Name for E2E test discovery
        System.Windows.Automation.AutomationProperties.SetName(panel, displayName);

        double left = 100, top = 100;

        if (_savedConfigs.TryGetValue(widget.WidgetId, out var savedConfig))
        {
            widget.UpdateConfiguration(savedConfig);

            if (!double.IsNaN(savedConfig.OverlayLeft) && !double.IsNaN(savedConfig.OverlayTop))
            {
                left = savedConfig.OverlayLeft;
                top = savedConfig.OverlayTop;
            }
        }

        _activeHostPanels[instanceId] = panel;
        _overlayHost!.AddWidget(panel, left, top);
    }

    public async Task RemoveWidgetInstance(string instanceId)
    {
        if (_activeHostPanels.TryGetValue(instanceId, out var panel))
        {
            _overlayHost?.RemoveWidget(panel);
            _activeHostPanels.Remove(instanceId);
        }

        if (_activeWidgets.TryGetValue(instanceId, out var widget))
        {
            await widget.StopAsync();
            _activeWidgets.Remove(instanceId);
        }
    }

    public void RegisterWidget(WidgetMetadata metadata)
    {
        _widgetRegistry.RegisterWidget(metadata);
        LoadWidgetLibrary();
    }

    public bool UnregisterWidget(string widgetId)
    {
        var result = _widgetRegistry.UnregisterWidget(widgetId);
        if (result)
        {
            LoadWidgetLibrary();
        }

        return result;
    }

    public void SaveConfiguration()
    {
        var activeWidgetIds = _activeWidgets.Values
            .Select(w => w.WidgetId)
            .Distinct()
            .ToList();

        _persistenceService.Save(_savedConfigs, activeWidgetIds);
    }

    public async Task LoadAndRestoreConfiguration()
    {
        var state = _persistenceService.Load();
        if (state == null)
            return;

        // Restore saved configs
        foreach (var (widgetId, element) in state.WidgetConfigs)
        {
            var config = _persistenceService.DeserializeConfig(widgetId, element);
            if (config != null)
            {
                _savedConfigs[widgetId] = config;
            }
        }

        // Restore active widgets
        _isRestoring = true;
        var registeredWidgets = _widgetRegistry.GetRegisteredWidgets().ToList();
        foreach (var widgetId in state.ActiveWidgets)
        {
            var metadata = registeredWidgets.FirstOrDefault(w => w.WidgetId == widgetId);
            if (metadata == null)
                continue;

            try
            {
                var widgetInstance = _serviceProvider.GetService(metadata.WidgetType) as IWidget;
                if (widgetInstance == null)
                    continue;

                string instanceId = $"{widgetId}-{Guid.NewGuid():N}";
                _activeWidgets[instanceId] = widgetInstance;

                await widgetInstance.StartAsync();
                ShowWidgetOverlay(widgetInstance, metadata.DisplayName, instanceId);

                // Set toggle on in library
                var libraryItem = WidgetLibraryItems.FirstOrDefault(i => i.WidgetId == widgetId);
                if (libraryItem != null)
                    libraryItem.IsEnabled = true;

                // Create card
                var iconKey = WidgetIconKeys.GetValueOrDefault(widgetId, "Icon.Gauge");
                var card = new ActiveWidgetCard(widgetId, metadata.DisplayName, iconKey);
                ActiveWidgetCards.Add(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring widget {WidgetId}", widgetId);
            }
        }
        _isRestoring = false;
    }
}
