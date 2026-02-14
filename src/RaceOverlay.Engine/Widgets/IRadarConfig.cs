using RaceOverlay.Core.Widgets;

namespace RaceOverlay.Engine.Widgets;

public interface IRadarConfig : IWidgetConfiguration
{
    string ConfigurationType => "RadarConfig";
    double OverlayLeft { get; set; }
    double OverlayTop { get; set; }
    double RangeMeters { get; set; }
    int UpdateIntervalMs { get; set; }
    bool UseMockData { get; set; }
    string PlayerColor { get; set; }
    string OpponentColor { get; set; }
}
