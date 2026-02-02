namespace RaceOverlay.Core.Widgets;

/// <summary>
/// Configuration interface for the Relative Overlay widget.
/// Controls how the relative overlay displays drivers on track.
/// </summary>
public interface IRelativeOverlayConfig : IWidgetConfiguration
{
    /// <summary>
    /// Gets the configuration type name for serialization.
    /// </summary>
    string IWidgetConfiguration.ConfigurationType => "RelativeOverlay";

    /// <summary>
    /// Number of drivers to display ahead of the player.
    /// </summary>
    int DriversAhead { get; set; }

    /// <summary>
    /// Number of drivers to display behind the player.
    /// </summary>
    int DriversBehind { get; set; }

    /// <summary>
    /// Whether to use mock data (for development) or real telemetry data.
    /// </summary>
    bool UseMockData { get; set; }

    /// <summary>
    /// Update frequency for relative positions (in milliseconds).
    /// </summary>
    int UpdateIntervalMs { get; set; }
}
