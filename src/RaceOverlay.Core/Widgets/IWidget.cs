namespace RaceOverlay.Core.Widgets;

/// <summary>
/// Represents a widget that can be displayed as an overlay in the racing application.
/// Widgets are modular components that display specific information (e.g., timing, radar, inputs).
/// </summary>
public interface IWidget
{
    /// <summary>
    /// Unique identifier for this widget type.
    /// </summary>
    string WidgetId { get; }

    /// <summary>
    /// Display name shown in the UI for this widget.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Description of what this widget does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the current configuration for this widget instance.
    /// </summary>
    IWidgetConfiguration Configuration { get; }

    /// <summary>
    /// Updates the widget configuration.
    /// </summary>
    void UpdateConfiguration(IWidgetConfiguration configuration);

    /// <summary>
    /// Called when the widget should start receiving data updates.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the widget should stop receiving data updates.
    /// </summary>
    Task StopAsync();
}
