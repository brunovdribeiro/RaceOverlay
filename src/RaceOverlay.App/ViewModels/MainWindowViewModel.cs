using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RaceOverlay.App.Models;
using RaceOverlay.App.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Widgets;
using Microsoft.Extensions.DependencyInjection;

namespace RaceOverlay.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IWidgetRegistry _widgetRegistry;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigurationPersistenceService _persistenceService = new();
    private readonly Dictionary<string, IWidget> _activeWidgets = new();
    private readonly Dictionary<string, WidgetOverlayWindow> _activeWindows = new();
    private readonly Dictionary<string, IWidgetConfiguration> _savedConfigs = new();
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

    public MainWindowViewModel(IWidgetRegistry widgetRegistry, IServiceProvider serviceProvider)
    {
        _widgetRegistry = widgetRegistry ?? throw new ArgumentNullException(nameof(widgetRegistry));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

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

    private void LoadConfigForWidget(string widgetId)
    {
        if (widgetId == "fuel-calculator")
        {
            if (_savedConfigs.TryGetValue(widgetId, out var saved) && saved is IFuelCalculatorConfig fuelConfig)
                LoadConfigFromFuelWidget(fuelConfig);
            else
            {
                var instance = _activeWidgets.Values.FirstOrDefault(w => w.WidgetId == widgetId);
                if (instance?.Configuration is IFuelCalculatorConfig config)
                    LoadConfigFromFuelWidget(config);
                else
                    LoadConfigFromFuelWidget(new FuelCalculatorConfig());
            }
        }
        else if (widgetId == "inputs")
        {
            if (_savedConfigs.TryGetValue(widgetId, out var saved) && saved is IInputsConfig inputsConfig)
                LoadConfigFromInputsWidget(inputsConfig);
            else
            {
                var instance = _activeWidgets.Values.FirstOrDefault(w => w.WidgetId == widgetId);
                if (instance?.Configuration is IInputsConfig config)
                    LoadConfigFromInputsWidget(config);
                else
                    LoadConfigFromInputsWidget(new InputsConfig());
            }
        }
        else if (widgetId == "input-trace")
        {
            if (_savedConfigs.TryGetValue(widgetId, out var saved) && saved is IInputTraceConfig inputTraceConfig)
                LoadConfigFromInputTraceWidget(inputTraceConfig);
            else
            {
                var instance = _activeWidgets.Values.FirstOrDefault(w => w.WidgetId == widgetId);
                if (instance?.Configuration is IInputTraceConfig config)
                    LoadConfigFromInputTraceWidget(config);
                else
                    LoadConfigFromInputTraceWidget(new InputTraceConfig());
            }
        }
        else if (widgetId == "standings")
        {
            if (_savedConfigs.TryGetValue(widgetId, out var saved) && saved is IStandingsConfig standingsConfig)
                LoadConfigFromStandingsWidget(standingsConfig);
            else
            {
                var instance = _activeWidgets.Values.FirstOrDefault(w => w.WidgetId == widgetId);
                if (instance?.Configuration is IStandingsConfig config)
                    LoadConfigFromStandingsWidget(config);
                else
                    LoadConfigFromStandingsWidget(new StandingsConfig());
            }
        }
        else if (widgetId == "lap-timer")
        {
            if (_savedConfigs.TryGetValue(widgetId, out var saved) && saved is ILapTimerConfig lapTimerConfig)
                LoadConfigFromLapTimerWidget(lapTimerConfig);
            else
            {
                var instance = _activeWidgets.Values.FirstOrDefault(w => w.WidgetId == widgetId);
                if (instance?.Configuration is ILapTimerConfig config)
                    LoadConfigFromLapTimerWidget(config);
                else
                    LoadConfigFromLapTimerWidget(new LapTimerConfig());
            }
        }
        else if (widgetId == "track-map")
        {
            if (_savedConfigs.TryGetValue(widgetId, out var saved) && saved is ITrackMapConfig trackMapConfig)
                LoadConfigFromTrackMapWidget(trackMapConfig);
            else
            {
                var instance = _activeWidgets.Values.FirstOrDefault(w => w.WidgetId == widgetId);
                if (instance?.Configuration is ITrackMapConfig config)
                    LoadConfigFromTrackMapWidget(config);
                else
                    LoadConfigFromTrackMapWidget(new TrackMapConfig());
            }
        }
        else if (widgetId == "weather")
        {
            if (_savedConfigs.TryGetValue(widgetId, out var saved) && saved is IWeatherConfig weatherConfig)
                LoadConfigFromWeatherWidget(weatherConfig);
            else
            {
                var instance = _activeWidgets.Values.FirstOrDefault(w => w.WidgetId == widgetId);
                if (instance?.Configuration is IWeatherConfig config)
                    LoadConfigFromWeatherWidget(config);
                else
                    LoadConfigFromWeatherWidget(new WeatherConfig());
            }
        }
        else
        {
            // Relative Overlay
            if (_savedConfigs.TryGetValue(widgetId, out var saved) && saved is IRelativeOverlayConfig relConfig)
                LoadConfigFromRelativeWidget(relConfig);
            else
            {
                var instance = _activeWidgets.Values.FirstOrDefault(w => w.WidgetId == widgetId);
                if (instance?.Configuration is IRelativeOverlayConfig config)
                    LoadConfigFromRelativeWidget(config);
                else
                    LoadConfigFromRelativeWidget(new RelativeOverlayConfig());
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

    private void PushRelativeConfigToActiveWidgets()
    {
        if (SelectedConfigWidgetId != "relative-overlay") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedConfigWidgetId, out var existing) && existing is RelativeOverlayConfig existingRel)
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

        _savedConfigs[SelectedConfigWidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.ApplyColumnVisibility(config);
            }
        }
    }

    private void PushInputsConfigToActiveWidgets()
    {
        if (SelectedConfigWidgetId != "inputs") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedConfigWidgetId, out var existing) && existing is InputsConfig existingInputs)
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

        _savedConfigs[SelectedConfigWidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.ApplyInputsConfig(config);
            }
        }
    }

    private void PushInputTraceConfigToActiveWidgets()
    {
        if (SelectedConfigWidgetId != "input-trace") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedConfigWidgetId, out var existing) && existing is InputTraceConfig existingTrace)
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

        _savedConfigs[SelectedConfigWidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.ApplyInputTraceConfig(config);
            }
        }
    }

    private void PushStandingsConfigToActiveWidgets()
    {
        if (SelectedConfigWidgetId != "standings") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedConfigWidgetId, out var existing) && existing is StandingsConfig existingStandings)
        {
            left = existingStandings.OverlayLeft;
            top = existingStandings.OverlayTop;
        }

        var config = new StandingsConfig
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
            OverlayLeft = left,
            OverlayTop = top
        };

        _savedConfigs[SelectedConfigWidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.ApplyStandingsConfig(config);
            }
        }
    }

    private void PushLapTimerConfigToActiveWidgets()
    {
        if (SelectedConfigWidgetId != "lap-timer") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedConfigWidgetId, out var existing) && existing is LapTimerConfig existingLapTimer)
        {
            left = existingLapTimer.OverlayLeft;
            top = existingLapTimer.OverlayTop;
        }

        var config = new LapTimerConfig
        {
            UpdateIntervalMs = LapTimerUpdateIntervalMs,
            ShowDeltaToBest = LapTimerShowDeltaToBest,
            ShowLastLap = LapTimerShowLastLap,
            ShowBestLap = LapTimerShowBestLap,
            ShowDeltaLastBest = LapTimerShowDeltaLastBest,
            OverlayLeft = left,
            OverlayTop = top
        };

        _savedConfigs[SelectedConfigWidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.ApplyLapTimerConfig(config);
            }
        }
    }

    private void PushTrackMapConfigToActiveWidgets()
    {
        if (SelectedConfigWidgetId != "track-map") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedConfigWidgetId, out var existing) && existing is TrackMapConfig existingTrackMap)
        {
            left = existingTrackMap.OverlayLeft;
            top = existingTrackMap.OverlayTop;
        }

        var config = new TrackMapConfig
        {
            UpdateIntervalMs = TrackMapUpdateIntervalMs,
            ShowDriverNames = TrackMapShowDriverNames,
            ShowPitStatus = TrackMapShowPitStatus,
            OverlayLeft = left,
            OverlayTop = top
        };

        _savedConfigs[SelectedConfigWidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.ApplyTrackMapConfig(config);
            }
        }
    }

    private void PushWeatherConfigToActiveWidgets()
    {
        if (SelectedConfigWidgetId != "weather") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedConfigWidgetId, out var existing) && existing is WeatherConfig existingWeather)
        {
            left = existingWeather.OverlayLeft;
            top = existingWeather.OverlayTop;
        }

        var config = new WeatherConfig
        {
            UpdateIntervalMs = WeatherUpdateIntervalMs,
            ShowWind = WeatherShowWind,
            ShowForecast = WeatherShowForecast,
            OverlayLeft = left,
            OverlayTop = top
        };

        _savedConfigs[SelectedConfigWidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.ApplyWeatherConfig(config);
            }
        }
    }

    private void PushFuelConfigToActiveWidgets()
    {
        if (SelectedConfigWidgetId != "fuel-calculator") return;

        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedConfigWidgetId, out var existing) && existing is FuelCalculatorConfig existingFuel)
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

        _savedConfigs[SelectedConfigWidgetId] = config;

        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedConfigWidgetId))
            {
                kv.Value.ApplyFuelConfig(config);
            }
        }
    }

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
            else if (config is StandingsConfig standingsConfig)
            {
                standingsConfig.OverlayLeft = left;
                standingsConfig.OverlayTop = top;
            }
            else if (config is LapTimerConfig lapTimerConfig)
            {
                lapTimerConfig.OverlayLeft = left;
                lapTimerConfig.OverlayTop = top;
            }
            else if (config is TrackMapConfig trackMapConfig)
            {
                trackMapConfig.OverlayLeft = left;
                trackMapConfig.OverlayTop = top;
            }
            else if (config is WeatherConfig weatherConfig)
            {
                weatherConfig.OverlayLeft = left;
                weatherConfig.OverlayTop = top;
            }
        }
        else
        {
            if (widgetId == "fuel-calculator")
                _savedConfigs[widgetId] = new FuelCalculatorConfig { OverlayLeft = left, OverlayTop = top };
            else if (widgetId == "inputs")
                _savedConfigs[widgetId] = new InputsConfig { OverlayLeft = left, OverlayTop = top };
            else if (widgetId == "input-trace")
                _savedConfigs[widgetId] = new InputTraceConfig { OverlayLeft = left, OverlayTop = top };
            else if (widgetId == "standings")
                _savedConfigs[widgetId] = new StandingsConfig { OverlayLeft = left, OverlayTop = top };
            else if (widgetId == "lap-timer")
                _savedConfigs[widgetId] = new LapTimerConfig { OverlayLeft = left, OverlayTop = top };
            else if (widgetId == "track-map")
                _savedConfigs[widgetId] = new TrackMapConfig { OverlayLeft = left, OverlayTop = top };
            else if (widgetId == "weather")
                _savedConfigs[widgetId] = new WeatherConfig { OverlayLeft = left, OverlayTop = top };
            else
                _savedConfigs[widgetId] = new RelativeOverlayConfig { OverlayLeft = left, OverlayTop = top };
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
            System.Diagnostics.Debug.WriteLine($"Error adding widget: {ex.Message}");
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

    private void ShowWidgetOverlay(IWidget widget, string displayName, string instanceId)
    {
        var overlayWindow = new WidgetOverlayWindow
        {
            Title = displayName,
            Widget = widget,
            InstanceId = instanceId,
            ViewModel = this
        };

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
            else if (savedConfig is StandingsConfig standingsConfig)
            {
                savedLeft = standingsConfig.OverlayLeft;
                savedTop = standingsConfig.OverlayTop;
            }
            else if (savedConfig is LapTimerConfig lapTimerConfig)
            {
                savedLeft = lapTimerConfig.OverlayLeft;
                savedTop = lapTimerConfig.OverlayTop;
            }
            else if (savedConfig is TrackMapConfig trackMapConfig)
            {
                savedLeft = trackMapConfig.OverlayLeft;
                savedTop = trackMapConfig.OverlayTop;
            }
            else if (savedConfig is WeatherConfig weatherConfig)
            {
                savedLeft = weatherConfig.OverlayLeft;
                savedTop = weatherConfig.OverlayTop;
            }

            if (!double.IsNaN(savedLeft) && !double.IsNaN(savedTop))
            {
                overlayWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                overlayWindow.Left = savedLeft;
                overlayWindow.Top = savedTop;
            }
        }

        _activeWindows[instanceId] = overlayWindow;

        overlayWindow.Closed += (s, e) =>
        {
            _activeWindows.Remove(instanceId);
        };

        overlayWindow.Show();
    }

    public async Task RemoveWidgetInstance(string instanceId)
    {
        if (_activeWindows.TryGetValue(instanceId, out var window))
        {
            window.Close();
            _activeWindows.Remove(instanceId);
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
                System.Diagnostics.Debug.WriteLine($"Error restoring widget {widgetId}: {ex.Message}");
            }
        }
        _isRestoring = false;
    }
}
