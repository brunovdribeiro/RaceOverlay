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

    [ObservableProperty]
    private WidgetMetadata? selectedWidget;

    [ObservableProperty]
    private ObservableCollection<WidgetMetadata> availableWidgets = new();

    [ObservableProperty]
    private string configurationDisplayText = string.Empty;

    [ObservableProperty]
    private bool isWidgetSelected;

    /// <summary>
    /// Initializes a new instance of the MainWindowViewModel.
    /// </summary>
    /// <param name="widgetRegistry">The widget registry service.</param>
    /// <param name="serviceProvider">The dependency injection service provider.</param>
    public MainWindowViewModel(IWidgetRegistry widgetRegistry, IServiceProvider serviceProvider)
    {
        _widgetRegistry = widgetRegistry ?? throw new ArgumentNullException(nameof(widgetRegistry));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        
        // Load available widgets from the registry
        LoadAvailableWidgets();
    }

    /// <summary>
    /// Called when selected widget changes to display its configuration.
    /// </summary>
    partial void OnSelectedWidgetChanged(WidgetMetadata? value)
    {
        if (value == null)
        {
            IsWidgetSelected = false;
            ConfigurationDisplayText = string.Empty;
            return;
        }

        IsWidgetSelected = true;
        ConfigurationDisplayText = GenerateConfigurationText(value);
    }

    /// <summary>
    /// Generates a human-readable configuration display text.
    /// </summary>
    private string GenerateConfigurationText(WidgetMetadata metadata)
    {
        var lines = new List<string>
        {
            "═══════════════════════════════",
            $"Widget: {metadata.DisplayName}",
            $"ID: {metadata.WidgetId}",
            $"Version: {metadata.Version}",
            $"Author: {metadata.Author ?? "Unknown"}",
            "",
            "Description:",
            metadata.Description,
            "",
            "Configuration Options:",
            "  • DriversAhead: 3 drivers",
            "  • DriversBehind: 3 drivers",
            "  • UseMockData: true (for development)",
            "  • UpdateIntervalMs: 500",
            "",
            "Status: Ready to deploy",
            "═══════════════════════════════",
        };

        return string.Join(Environment.NewLine, lines);
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

        // Track the window
        _activeWindows[instanceId] = overlayWindow;

        // Register with drag service for hotkey management
        WidgetDragService.Instance.RegisterWindow(overlayWindow);

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
