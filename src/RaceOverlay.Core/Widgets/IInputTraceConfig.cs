namespace RaceOverlay.Core.Widgets;

public interface IInputTraceConfig : IWidgetConfiguration
{
    string IWidgetConfiguration.ConfigurationType => "InputTrace";

    int UpdateIntervalMs { get; set; }
    bool UseMockData { get; set; }
    double OverlayLeft { get; set; }
    double OverlayTop { get; set; }
    string ThrottleColor { get; set; }
    string BrakeColor { get; set; }
    int HistorySeconds { get; set; }
}
