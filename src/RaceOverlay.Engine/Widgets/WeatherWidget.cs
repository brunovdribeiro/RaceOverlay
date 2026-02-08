using RaceOverlay.Core.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

public class WeatherConfig : IWeatherConfig
{
    public int UpdateIntervalMs { get; set; } = 2000;
    public bool UseMockData { get; set; } = false;
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;
    public bool ShowWind { get; set; } = true;
    public bool ShowForecast { get; set; } = true;
}

public class WeatherWidget : IWidget
{
    private WeatherConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private readonly Random _random = new();
    private readonly ILiveTelemetryService? _telemetryService;

    // Mock weather state
    private int _weatherState;
    private double _trackTemp;
    private double _airTemp;
    private int _humidity;
    private double _windSpeed;
    private int _windDirectionIndex;
    private double _rainChance;
    private int _currentLap;

    private static readonly string[] WeatherStates = ["Clear", "Overcast", "Light Rain", "Heavy Rain"];
    private static readonly string[] WindDirections =
        ["N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"];

    public event Action? DataUpdated;

    public string WidgetId => "weather";
    public string DisplayName => "Weather";
    public string Description => "Displays track/air temperature, weather conditions, humidity, wind, and rain forecast.";
    public IWidgetConfiguration Configuration => _configuration;

    public int TotalLaps { get; private set; } = 24;

    private bool UseLiveData => !_configuration.UseMockData
                                && _telemetryService?.IsConnected == true;

    public WeatherWidget(ILiveTelemetryService? telemetryService = null)
    {
        _configuration = new WeatherConfig();
        _telemetryService = telemetryService;
    }

    public void UpdateConfiguration(IWidgetConfiguration configuration)
    {
        if (configuration is WeatherConfig config)
        {
            _configuration = config;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        if (!UseLiveData)
        {
            InitializeMockData();
        }

        _updateTask = UpdateLoopAsync(_cancellationTokenSource.Token);
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource?.Cancel();

        if (_updateTask != null)
        {
            try
            {
                await _updateTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        _cancellationTokenSource?.Dispose();
    }

    private void InitializeMockData()
    {
        _weatherState = 0;
        _trackTemp = 38.0;
        _airTemp = 22.0;
        _humidity = 45;
        _windSpeed = 12.0;
        _windDirectionIndex = _random.Next(WindDirections.Length);
        _rainChance = 10.0;
        _currentLap = 1;
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!UseLiveData)
                {
                    UpdateMockWeather();
                }

                DataUpdated?.Invoke();
                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void UpdateMockWeather()
    {
        if (_random.Next(200) == 0)
        {
            if (_weatherState == 0)
                _weatherState = 1;
            else if (_weatherState == 3)
                _weatherState = 2;
            else
                _weatherState += _random.Next(2) == 0 ? -1 : 1;
        }

        _trackTemp += (_random.NextDouble() - 0.5) * 0.2;
        _airTemp += (_random.NextDouble() - 0.5) * 0.1;

        double trackTempTarget = _weatherState switch
        {
            0 => 38.0, 1 => 34.0, 2 => 28.0, 3 => 24.0, _ => 38.0
        };
        _trackTemp += (trackTempTarget - _trackTemp) * 0.01;

        int humidityTarget = _weatherState switch
        {
            0 => 45, 1 => 62, 2 => 80, 3 => 90, _ => 45
        };
        _humidity += Math.Sign(humidityTarget - _humidity) * _random.Next(0, 2);
        _humidity = Math.Clamp(_humidity, 20, 99);

        if (_random.Next(50) == 0)
        {
            _windDirectionIndex = (_windDirectionIndex + (_random.Next(2) == 0 ? -1 : 1) + WindDirections.Length) % WindDirections.Length;
        }
        _windSpeed += (_random.NextDouble() - 0.5) * 0.5;
        _windSpeed = Math.Clamp(_windSpeed, 5.0, 25.0);

        double rainTarget = _weatherState switch
        {
            0 => 10.0, 1 => 35.0, 2 => 70.0, 3 => 92.0, _ => 10.0
        };
        _rainChance += (rainTarget - _rainChance) * 0.05 + (_random.NextDouble() - 0.5) * 2.0;
        _rainChance = Math.Clamp(_rainChance, 0.0, 100.0);

        if (_random.Next(30) == 0 && _currentLap < TotalLaps)
        {
            _currentLap++;
        }
    }

    public WeatherData GetWeatherData()
    {
        if (UseLiveData)
        {
            return GetLiveWeatherData();
        }

        return GetMockWeatherData();
    }

    private WeatherData GetLiveWeatherData()
    {
        var ts = _telemetryService!;

        float trackTemp = ts.GetFloat("TrackTempCrew");
        float airTemp = ts.GetFloat("AirTemp");
        float windVel = ts.GetFloat("WindVel");
        float windDir = ts.GetFloat("WindDir");
        float humidity = ts.GetFloat("RelativeHumidity");
        int skies = ts.GetInt("Skies");
        int precipitation = ts.GetInt("Precipitation");

        // Convert wind direction from radians to compass
        double windDegrees = windDir * 180.0 / Math.PI;
        if (windDegrees < 0) windDegrees += 360;
        int windIdx = (int)Math.Round(windDegrees / 22.5) % 16;
        string windDirection = WindDirections[windIdx];

        // Map skies + precipitation to conditions string
        string conditions;
        if (precipitation > 0)
        {
            conditions = skies >= 2 ? "Heavy Rain" : "Light Rain";
        }
        else
        {
            conditions = skies switch
            {
                0 => "Clear",
                1 => "Partly Cloudy",
                2 => "Overcast",
                3 => "Overcast",
                _ => "Clear"
            };
        }

        int currentLap = ts.GetInt("Lap");
        int totalLaps = ts.SessionLaps;

        return new WeatherData
        {
            Conditions = conditions,
            TrackTempC = Math.Round(trackTemp, 1),
            AirTempC = Math.Round(airTemp, 1),
            HumidityPercent = (int)Math.Round(humidity),
            WindSpeedKph = Math.Round(windVel * 3.6, 0),
            WindDirection = windDirection,
            RainChancePercent = precipitation > 0 ? 90 : skies >= 2 ? 40 : 10,
            ForecastConditions = conditions, // use current as forecast
            ForecastMinutes = -1,
            CurrentLap = currentLap,
            TotalLaps = totalLaps
        };
    }

    private WeatherData GetMockWeatherData()
    {
        string forecastConditions;
        int forecastMinutes;

        if (_weatherState < 3 && _rainChance > 50)
        {
            forecastConditions = WeatherStates[_weatherState + 1];
            forecastMinutes = 5 + _random.Next(26);
        }
        else if (_weatherState > 0 && _rainChance < 20)
        {
            forecastConditions = WeatherStates[_weatherState - 1];
            forecastMinutes = 5 + _random.Next(26);
        }
        else
        {
            forecastConditions = WeatherStates[_weatherState];
            forecastMinutes = -1;
        }

        return new WeatherData
        {
            Conditions = WeatherStates[_weatherState],
            TrackTempC = Math.Round(_trackTemp, 1),
            AirTempC = Math.Round(_airTemp, 1),
            HumidityPercent = _humidity,
            WindSpeedKph = Math.Round(_windSpeed, 0),
            WindDirection = WindDirections[_windDirectionIndex],
            RainChancePercent = Math.Round(_rainChance, 0),
            ForecastConditions = forecastConditions,
            ForecastMinutes = forecastMinutes,
            CurrentLap = _currentLap,
            TotalLaps = TotalLaps
        };
    }
}
