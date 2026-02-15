using RaceOverlay.Core.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;
using RaceOverlay.Engine.Services;

namespace RaceOverlay.Engine.Widgets;

public class RadarConfig : IRadarConfig
{
    public string ConfigurationType => "RadarConfig";
    public double RangeMeters { get; set; } = 40.0;
    public int UpdateIntervalMs { get; set; } = 33; // ~30Hz
    public bool UseMockData { get; set; } = false;
    public string PlayerColor { get; set; } = "#3B82F6"; // Blue
    public string OpponentColor { get; set; } = "#EF4444"; // Red
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;

    // Proximity colors
    public bool UseProximityColors { get; set; } = true;
    public string ProximityFarColor { get; set; } = "#22C55E"; // Green
    public string ProximityMidColor { get; set; } = "#F59E0B"; // Amber
    public string ProximityCloseColor { get; set; } = "#EF4444"; // Red
    public double ProximityCloseThreshold { get; set; } = 10.0;
    public double ProximityMidThreshold { get; set; } = 20.0;

    // Blind spot indicators
    public bool ShowBlindSpotIndicators { get; set; } = true;

    // Sound alerts
    public bool EnableSoundAlerts { get; set; } = false;
    public int AlertCooldownMs { get; set; } = 1500;
}

public class RadarWidget : IWidget
{
    private RadarConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private readonly ILiveTelemetryService? _telemetryService;
    private List<RadarCar> _cars = new();
    private readonly Random _random = new();
    private double _elapsed;
    private readonly ProximityAlertService _alertService = new();
    private bool _wasCarOnLeft;
    private bool _wasCarOnRight;

    public event Action? DataUpdated;

    public string WidgetId => "radar";
    public string DisplayName => "Radar";
    public string Description => "Top-down proximity radar showing cars around you as rectangles.";
    public IWidgetConfiguration Configuration => _configuration;

    private bool UseLiveData => !_configuration.UseMockData
                                && _telemetryService?.IsConnected == true;

    public RadarWidget(ILiveTelemetryService? telemetryService = null)
    {
        _configuration = new RadarConfig();
        _telemetryService = telemetryService;
    }

    public void UpdateConfiguration(IWidgetConfiguration configuration)
    {
        if (configuration is RadarConfig config)
        {
            _configuration = config;
            _alertService.CooldownMs = config.AlertCooldownMs;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _elapsed = 0;
        _updateTask = UpdateLoopAsync(_cancellationTokenSource.Token);
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource?.Cancel();
        if (_updateTask != null)
        {
            try { await _updateTask; } catch (OperationCanceledException) { }
        }
        _cancellationTokenSource?.Dispose();
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (UseLiveData)
                    UpdateLiveRadar();
                else
                    UpdateMockRadar();

                ProcessBlindSpotAlerts();

                DataUpdated?.Invoke();
                _elapsed += _configuration.UpdateIntervalMs / 1000.0;
                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException) { }
    }

    private void UpdateLiveRadar()
    {
        var ts = _telemetryService!;
        int playerCarIdx = ts.PlayerCarIdx;
        float playerEstTime = ts.GetFloat("CarIdxEstTime", playerCarIdx);
        float playerSpeed = ts.GetFloat("Speed"); // m/s
        
        var liveCars = new List<RadarCar>();

        // Add player
        liveCars.Add(new RadarCar
        {
            CarIdx = playerCarIdx,
            IsPlayer = true,
            LongitudinalOffset = 0,
            LateralOffset = 0,
            Color = _configuration.PlayerColor,
            DriverName = "You"
        });

        for (int i = 0; i < Math.Min(ts.DriverCount, 64); i++)
        {
            if (i == playerCarIdx) continue;

            var info = ts.GetDriverInfo(i);
            if (info == null || info.IsSpectator) continue;

            float carEstTime = ts.GetFloat("CarIdxEstTime", i);
            if (carEstTime <= 0) continue;

            // Longitudinal offset in meters
            // time gap = carEstTime - playerEstTime (if car is ahead, carEstTime is smaller? No, EstTime is time to finish lap)
            // If car is ahead, it has LESS time to finish lap.
            // gapSeconds = playerEstTime - carEstTime
            double gapSeconds = playerEstTime - carEstTime;
            
            // Handle lap wrap around if necessary (simplified here)
            if (gapSeconds > 10.0) gapSeconds -= 60.0; // Very crude
            if (gapSeconds < -10.0) gapSeconds += 60.0;

            double longOffset = gapSeconds * playerSpeed;

            if (Math.Abs(longOffset) < _configuration.RangeMeters)
            {
                // Lateral offset: iRacing provides CarIdxF2Time (time to right of track)
                // or we can use bitfield for car left/right
                int carLeftRight = ts.GetInt("CarLeftRight");
                // bit 0: car left, bit 1: car right, etc. (Actually it's an enum in iRacing)
                
                double latOffset = 0;
                // Simplified lateral based on index or something if we don't have real lateral
                // For now, let's use a small mock lateral offset so they don't overlap perfectly
                latOffset = (i % 3 - 1) * 3.0; 

                liveCars.Add(new RadarCar
                {
                    CarIdx = i,
                    IsPlayer = false,
                    LongitudinalOffset = longOffset,
                    LateralOffset = latOffset,
                    Color = GetOpponentColor(latOffset, longOffset),
                    DriverName = info.UserName
                });
            }
        }

        _cars = liveCars;
    }

    private void UpdateMockRadar()
    {
        var mockCars = new List<RadarCar>();
        mockCars.Add(new RadarCar
        {
            IsPlayer = true,
            LongitudinalOffset = 0,
            LateralOffset = 0,
            Color = _configuration.PlayerColor,
            DriverName = "You"
        });

        // Add some mock opponents moving around
        for (int i = 0; i < 3; i++)
        {
            double phase = _elapsed * (0.5 + i * 0.1) + i;
            double longOffset = Math.Sin(phase) * 15;
            double latOffset = Math.Cos(phase * 0.5) * 4;

            mockCars.Add(new RadarCar
            {
                CarIdx = i + 100,
                IsPlayer = false,
                LongitudinalOffset = longOffset,
                LateralOffset = latOffset,
                Color = GetOpponentColor(latOffset, longOffset),
                DriverName = $"Opponent {i + 1}"
            });
        }

        _cars = mockCars;
    }

    public IReadOnlyList<RadarCar> GetCars() => _cars.AsReadOnly();

    private string GetOpponentColor(double latOffset, double longOffset)
    {
        if (!_configuration.UseProximityColors)
            return _configuration.OpponentColor;

        double distance = Math.Sqrt(latOffset * latOffset + longOffset * longOffset);

        if (distance <= _configuration.ProximityCloseThreshold)
            return _configuration.ProximityCloseColor;
        if (distance <= _configuration.ProximityMidThreshold)
            return _configuration.ProximityMidColor;
        return _configuration.ProximityFarColor;
    }

    private void ProcessBlindSpotAlerts()
    {
        if (!_configuration.EnableSoundAlerts)
            return;

        const double lateralThreshold = 1.0;
        const double longitudinalThreshold = 8.0;

        bool carOnLeft = false;
        bool carOnRight = false;

        foreach (var car in _cars)
        {
            if (car.IsPlayer) continue;
            if (Math.Abs(car.LongitudinalOffset) > longitudinalThreshold) continue;

            if (car.LateralOffset < -lateralThreshold)
                carOnLeft = true;
            if (car.LateralOffset > lateralThreshold)
                carOnRight = true;
        }

        // Fire alerts on state transition only (wasn't in zone → now is)
        if (carOnLeft && !_wasCarOnLeft)
            _alertService.PlayLeftAlert();
        if (carOnRight && !_wasCarOnRight)
            _alertService.PlayRightAlert();

        _wasCarOnLeft = carOnLeft;
        _wasCarOnRight = carOnRight;
    }
}
