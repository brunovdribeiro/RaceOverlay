using CommunityToolkit.Mvvm.ComponentModel;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.ViewModels;

public partial class WeatherViewModel : ObservableObject
{
    [ObservableProperty]
    private string conditions = "Clear";

    [ObservableProperty]
    private double trackTempC;

    [ObservableProperty]
    private double airTempC;

    [ObservableProperty]
    private int humidityPercent;

    [ObservableProperty]
    private double windSpeedKph;

    [ObservableProperty]
    private string windDirection = "N";

    [ObservableProperty]
    private double rainChancePercent;

    [ObservableProperty]
    private string forecastConditions = "Clear";

    [ObservableProperty]
    private int forecastMinutes = -1;

    [ObservableProperty]
    private int currentLap;

    [ObservableProperty]
    private int totalLaps;

    [ObservableProperty]
    private bool showWind = true;

    [ObservableProperty]
    private bool showForecast = true;

    public void ApplyConfiguration(IWeatherConfig config)
    {
        ShowWind = config.ShowWind;
        ShowForecast = config.ShowForecast;
    }

    public void UpdateWeather(WeatherData data)
    {
        Conditions = data.Conditions;
        TrackTempC = data.TrackTempC;
        AirTempC = data.AirTempC;
        HumidityPercent = data.HumidityPercent;
        WindSpeedKph = data.WindSpeedKph;
        WindDirection = data.WindDirection;
        RainChancePercent = data.RainChancePercent;
        ForecastConditions = data.ForecastConditions;
        ForecastMinutes = data.ForecastMinutes;
        CurrentLap = data.CurrentLap;
        TotalLaps = data.TotalLaps;
    }
}
