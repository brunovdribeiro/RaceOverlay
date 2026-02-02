using RaceOverlay.Core.Widgets;

namespace RaceOverlay.Engine.Widgets;

/// <summary>
/// Registry for discovering, registering, and managing widget types and instances.
/// Implements the plugin system for widgets.
/// </summary>
public interface IWidgetRegistry
{
    /// <summary>
    /// Registers a widget type in the registry.
    /// </summary>
    /// <param name="metadata">Metadata describing the widget type.</param>
    void RegisterWidget(WidgetMetadata metadata);

    /// <summary>
    /// Gets metadata for all registered widgets.
    /// </summary>
    IReadOnlyList<WidgetMetadata> GetRegisteredWidgets();

    /// <summary>
    /// Gets metadata for a specific widget by ID.
    /// </summary>
    WidgetMetadata? GetWidgetMetadata(string widgetId);

    /// <summary>
    /// Creates an instance of a widget by its ID.
    /// </summary>
    IWidget? CreateWidget(string widgetId, IServiceProvider serviceProvider);

    /// <summary>
    /// Removes a widget from the registry.
    /// </summary>
    bool UnregisterWidget(string widgetId);
}
