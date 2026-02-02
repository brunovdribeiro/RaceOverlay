namespace RaceOverlay.Core.Widgets;

/// <summary>
/// Metadata describing a widget type. Used by the registry to store information
/// about available widgets without requiring instances.
/// </summary>
public class WidgetMetadata
{
    /// <summary>
    /// Unique identifier for the widget type.
    /// </summary>
    public required string WidgetId { get; init; }

    /// <summary>
    /// Display name of the widget.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Description of what the widget does.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The type that implements IWidget for this widget.
    /// </summary>
    public required Type WidgetType { get; init; }

    /// <summary>
    /// The type that implements IWidgetConfiguration for this widget.
    /// </summary>
    public required Type ConfigurationType { get; init; }

    /// <summary>
    /// Version of the widget.
    /// </summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    /// Author of the widget.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Gets a string representation of the widget metadata.
    /// </summary>
    public override string ToString() => $"{DisplayName} ({WidgetId}) v{Version}";
}
