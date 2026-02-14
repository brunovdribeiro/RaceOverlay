namespace RaceOverlay.Core.Widgets;

/// <summary>
/// Configuration interface for the Lap Timer widget.
/// Controls which timing sections are displayed.
/// </summary>
public interface ILapTimerConfig : IWidgetConfiguration
{
    string IWidgetConfiguration.ConfigurationType => "LapTimer";

    int UpdateIntervalMs { get; set; }
    bool UseMockData { get; set; }
    bool ShowDeltaToBest { get; set; }
    bool ShowLastLap { get; set; }
    bool ShowBestLap { get; set; }
    bool ShowDeltaLastBest { get; set; }
}
