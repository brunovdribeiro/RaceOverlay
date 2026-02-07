namespace RaceOverlay.Core.Widgets;

public interface IInputsConfig : IWidgetConfiguration
{
    string IWidgetConfiguration.ConfigurationType => "Inputs";

    int UpdateIntervalMs { get; set; }
    bool UseMockData { get; set; }
    double OverlayLeft { get; set; }
    double OverlayTop { get; set; }
    string ThrottleColor { get; set; }
    string BrakeColor { get; set; }
    string ClutchColor { get; set; }
    bool ShowClutch { get; set; }
}
