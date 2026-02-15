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

    // Proximity colors
    bool UseProximityColors { get; set; }
    string ProximityFarColor { get; set; }
    string ProximityMidColor { get; set; }
    string ProximityCloseColor { get; set; }
    double ProximityCloseThreshold { get; set; }
    double ProximityMidThreshold { get; set; }

    // Blind spot indicators
    bool ShowBlindSpotIndicators { get; set; }

    // Sound alerts
    bool EnableSoundAlerts { get; set; }
    int AlertCooldownMs { get; set; }
}
