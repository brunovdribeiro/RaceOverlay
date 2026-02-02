using System.Collections.Generic;
using System.Windows;

namespace RaceOverlay.App.Services;

/// <summary>
/// Service to manage widget dragging state and track open overlay windows.
/// </summary>
public class WidgetDragService
{
    private static WidgetDragService? _instance;
    private readonly List<WidgetOverlayWindow> _openWindows = new();
    private bool _isDraggingEnabled;

    public static WidgetDragService Instance
    {
        get => _instance ??= new WidgetDragService();
    }

    /// <summary>
    /// Gets or sets whether dragging is enabled for all widgets.
    /// </summary>
    public bool IsDraggingEnabled
    {
        get => _isDraggingEnabled;
        set
        {
            if (_isDraggingEnabled != value)
            {
                _isDraggingEnabled = value;
                UpdateAllWindows();
            }
        }
    }

    /// <summary>
    /// Registers an overlay window for drag management.
    /// </summary>
    public void RegisterWindow(WidgetOverlayWindow window)
    {
        if (!_openWindows.Contains(window))
        {
            _openWindows.Add(window);
            window.SetDraggingEnabled(_isDraggingEnabled);
        }
    }

    /// <summary>
    /// Unregisters an overlay window.
    /// </summary>
    public void UnregisterWindow(WidgetOverlayWindow window)
    {
        _openWindows.Remove(window);
    }

    /// <summary>
    /// Toggles dragging mode for all windows.
    /// </summary>
    public void ToggleDragging()
    {
        IsDraggingEnabled = !_isDraggingEnabled;
    }

    /// <summary>
    /// Updates all open windows with current dragging state.
    /// </summary>
    private void UpdateAllWindows()
    {
        foreach (var window in _openWindows.ToList())
        {
            if (window.IsLoaded)
            {
                window.SetDraggingEnabled(_isDraggingEnabled);
            }
        }
    }
}
