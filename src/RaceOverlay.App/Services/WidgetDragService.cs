using System.Collections.Generic;

namespace RaceOverlay.App.Services;

/// <summary>
/// Service to manage widget dragging state and track open widget host panels.
/// </summary>
public class WidgetDragService
{
    private static WidgetDragService? _instance;
    private readonly List<WidgetHostPanel> _openPanels = new();
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
                UpdateAllPanels();
            }
        }
    }

    /// <summary>
    /// Registers a widget host panel for drag management.
    /// </summary>
    public void RegisterPanel(WidgetHostPanel panel)
    {
        if (!_openPanels.Contains(panel))
        {
            _openPanels.Add(panel);
            panel.SetDraggingEnabled(_isDraggingEnabled);
        }
    }

    /// <summary>
    /// Unregisters a widget host panel.
    /// </summary>
    public void UnregisterPanel(WidgetHostPanel panel)
    {
        _openPanels.Remove(panel);
    }

    /// <summary>
    /// Toggles dragging mode for all panels.
    /// </summary>
    public void ToggleDragging()
    {
        IsDraggingEnabled = !_isDraggingEnabled;
    }

    /// <summary>
    /// Updates all open panels with current dragging state.
    /// </summary>
    private void UpdateAllPanels()
    {
        foreach (var panel in _openPanels.ToList())
        {
            if (panel.IsLoaded)
            {
                panel.SetDraggingEnabled(_isDraggingEnabled);
            }
        }
    }
}
