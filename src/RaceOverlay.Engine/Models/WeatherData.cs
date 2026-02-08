namespace RaceOverlay.Engine.Models;

public class WeatherData
{
    public string Conditions { get; set; } = "Clear";
    public double TrackTempC { get; set; }
    public double AirTempC { get; set; }
    public int HumidityPercent { get; set; }
    public double WindSpeedKph { get; set; }
    public string WindDirection { get; set; } = "N";
    public double RainChancePercent { get; set; }
    public string ForecastConditions { get; set; } = "Clear";
    public int ForecastMinutes { get; set; } = -1;
    public int CurrentLap { get; set; }
    public int TotalLaps { get; set; }
}
