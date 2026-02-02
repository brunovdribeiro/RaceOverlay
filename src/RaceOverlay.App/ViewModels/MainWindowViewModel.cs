using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.App.ViewModels;

/// <summary>
/// ViewModel for the main window that manages widget selection and configuration.
/// Uses MVVM Toolkit for observable properties and relay commands.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IWidgetRegistry _widgetRegistry;

    [ObservableProperty]
    private WidgetMetadata? selectedWidget;

    [ObservableProperty]
    private ObservableCollection<WidgetMetadata> availableWidgets = new();

    /// <summary>
    /// Initializes a new instance of the MainWindowViewModel.
    /// </summary>
    /// <param name="widgetRegistry">The widget registry service.</param>
    public MainWindowViewModel(IWidgetRegistry widgetRegistry)
    {
        _widgetRegistry = widgetRegistry ?? throw new ArgumentNullException(nameof(widgetRegistry));
        
        // Load available widgets from the registry
        LoadAvailableWidgets();
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
    private void AddWidget()
    {
        if (SelectedWidget == null)
        {
            // Could show a notification or error message
            return;
        }

        // TODO: Implement widget instantiation and addition to active widgets
        // This would involve:
        // 1. Creating an instance via the registry
        // 2. Adding it to a collection of active widgets
        // 3. Showing it in the overlay or widget panel
    }

    /// <summary>
    /// Command to remove the selected widget from the application.
    /// </summary>
    [RelayCommand]
    private void RemoveWidget()
    {
        if (SelectedWidget == null)
        {
            return;
        }

        // TODO: Implement widget removal
        // This would involve:
        // 1. Finding the active widget instance
        // 2. Stopping it (calling StopAsync)
        // 3. Removing it from the active widgets collection
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
