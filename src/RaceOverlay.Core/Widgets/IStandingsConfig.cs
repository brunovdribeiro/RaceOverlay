namespace RaceOverlay.Core.Widgets;

/// <summary>
/// Configuration interface for the Standings widget.
/// Controls how the race leaderboard displays drivers.
/// </summary>
public interface IStandingsConfig : IWidgetConfiguration
{
    string IWidgetConfiguration.ConfigurationType => "Standings";

    int UpdateIntervalMs { get; set; }
    bool UseMockData { get; set; }
    double OverlayLeft { get; set; }
    double OverlayTop { get; set; }
    bool ShowClassColor { get; set; }
    bool ShowBestLapTime { get; set; }
    bool ShowGap { get; set; }
    int MaxDrivers { get; set; }
}
