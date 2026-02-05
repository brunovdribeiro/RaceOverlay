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
    private readonly Dictionary<string, RelativeOverlayConfig> _savedConfigs = new();

    [ObservableProperty]
    private WidgetMetadata? selectedWidget;

    [ObservableProperty]
    private ObservableCollection<WidgetMetadata> availableWidgets = new();

    [ObservableProperty]
    private bool isWidgetSelected;

    // Column visibility toggles
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

    // Data settings
    [ObservableProperty]
    private int driversAhead = 3;

    [ObservableProperty]
    private int driversBehind = 3;

    [ObservableProperty]
    private int updateIntervalMs = 500;

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
            return;
        }

        IsWidgetSelected = true;

        // Priority: saved config > active instance > defaults
        if (_savedConfigs.TryGetValue(value.WidgetId, out var savedConfig))
        {
            LoadConfigFromWidget(savedConfig);
        }
        else
        {
            var activeInstance = _activeWidgets.Values
                .FirstOrDefault(w => w.WidgetId == value.WidgetId);

            if (activeInstance?.Configuration is IRelativeOverlayConfig config)
            {
                LoadConfigFromWidget(config);
            }
            else
            {
                var defaults = new RelativeOverlayConfig();
                LoadConfigFromWidget(defaults);
            }
        }
    }

    private void LoadConfigFromWidget(IRelativeOverlayConfig config)
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

    // Push config changes to active widget instances when toggles change
    partial void OnShowPositionChanged(bool value) => PushConfigToActiveWidgets();
    partial void OnShowClassColorChanged(bool value) => PushConfigToActiveWidgets();
    partial void OnShowDriverNameChanged(bool value) => PushConfigToActiveWidgets();
    partial void OnShowRatingChanged(bool value) => PushConfigToActiveWidgets();
    partial void OnShowStintChanged(bool value) => PushConfigToActiveWidgets();
    partial void OnShowLapTimeChanged(bool value) => PushConfigToActiveWidgets();
    partial void OnShowGapChanged(bool value) => PushConfigToActiveWidgets();
    partial void OnDriversAheadChanged(int value) => PushConfigToActiveWidgets();
    partial void OnDriversBehindChanged(int value) => PushConfigToActiveWidgets();
    partial void OnUpdateIntervalMsChanged(int value) => PushConfigToActiveWidgets();

    private void PushConfigToActiveWidgets()
    {
        if (SelectedWidget == null) return;

        // Preserve saved position if it exists
        double left = double.NaN, top = double.NaN;
        if (_savedConfigs.TryGetValue(SelectedWidget.WidgetId, out var existing))
        {
            left = existing.OverlayLeft;
            top = existing.OverlayTop;
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

        // Save snapshot
        _savedConfigs[SelectedWidget.WidgetId] = config;

        // Push to all active widget instances of this type
        foreach (var kv in _activeWidgets)
        {
            if (kv.Key.StartsWith(SelectedWidget.WidgetId))
            {
                kv.Value.UpdateConfiguration(config);
            }
        }

        // Also push column visibility to overlay view models
        foreach (var kv in _activeWindows)
        {
            if (kv.Key.StartsWith(SelectedWidget.WidgetId))
            {
                kv.Value.ApplyColumnVisibility(config);
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
            config.OverlayLeft = left;
            config.OverlayTop = top;
        }
        else
        {
            var newConfig = new RelativeOverlayConfig { OverlayLeft = left, OverlayTop = top };
            _savedConfigs[widgetId] = newConfig;
        }

        // Update position text if this widget is currently selected
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
            // Push saved config to the new widget instance
            widget.UpdateConfiguration(savedConfig);

            if (!double.IsNaN(savedConfig.OverlayLeft) && !double.IsNaN(savedConfig.OverlayTop))
            {
                overlayWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                overlayWindow.Left = savedConfig.OverlayLeft;
                overlayWindow.Top = savedConfig.OverlayTop;
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
