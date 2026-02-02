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
}
