namespace RaceOverlay.Core.Widgets;

public interface ITrackMapConfig : IWidgetConfiguration
{
    string IWidgetConfiguration.ConfigurationType => "TrackMap";

    int UpdateIntervalMs { get; set; }
    bool UseMockData { get; set; }
    bool ShowDriverNames { get; set; }
    bool ShowPitStatus { get; set; }
}
