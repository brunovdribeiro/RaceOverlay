namespace RaceOverlay.Core.Widgets;

public interface IWeatherConfig : IWidgetConfiguration
{
    string IWidgetConfiguration.ConfigurationType => "Weather";

    int UpdateIntervalMs { get; set; }
    bool UseMockData { get; set; }
    bool ShowWind { get; set; }
    bool ShowForecast { get; set; }
}
