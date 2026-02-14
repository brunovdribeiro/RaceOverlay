namespace RaceOverlay.Core.Widgets;

/// <summary>
/// Base interface for widget configuration objects.
/// Each widget type should implement a specific configuration interface derived from this.
/// </summary>
public interface IWidgetConfiguration
{
    /// <summary>
    /// Gets the type name of the configuration, used for serialization and type identification.
    /// </summary>
    string ConfigurationType { get; }

    /// <summary>
    /// Gets or sets the horizontal position of the widget overlay on screen.
    /// </summary>
    double OverlayLeft { get; set; }

    /// <summary>
    /// Gets or sets the vertical position of the widget overlay on screen.
    /// </summary>
    double OverlayTop { get; set; }
}
