using RaceOverlay.Core.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

public class InputsConfig : IInputsConfig
{
    public int UpdateIntervalMs { get; set; } = 16;
    public bool UseMockData { get; set; } = false;
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;
    public string ThrottleColor { get; set; } = "#22C55E";
    public string BrakeColor { get; set; } = "#EF4444";
    public string ClutchColor { get; set; } = "#3B82F6";
    public bool ShowClutch { get; set; } = false;
}

public class InputsWidget : IWidget
{
    private InputsConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private readonly ILiveTelemetryService? _telemetryService;

    // Mock state
    private double _elapsed;
    private readonly Random _random = new();

    public event Action? DataUpdated;

    public string WidgetId => "inputs";
    public string DisplayName => "Inputs";
    public string Description => "Visualizes throttle, brake, and steering telemetry in real time.";
    public IWidgetConfiguration Configuration => _configuration;

    private bool UseLiveData => !_configuration.UseMockData
                                && _telemetryService?.IsConnected == true;

    public InputsWidget(ILiveTelemetryService? telemetryService = null)
    {
        _configuration = new InputsConfig();
        _telemetryService = telemetryService;
    }

    public void UpdateConfiguration(IWidgetConfiguration configuration)
    {
        if (configuration is InputsConfig config)
        {
            _configuration = config;
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

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _elapsed += _configuration.UpdateIntervalMs / 1000.0;

                DataUpdated?.Invoke();

                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public InputsData GetInputsData()
    {
        if (UseLiveData)
        {
            return GetLiveInputsData();
        }

        return GetMockInputsData();
    }

    private InputsData GetLiveInputsData()
    {
        var ts = _telemetryService!;

        float steeringAngle = ts.GetFloat("SteeringWheelAngle");
        // Normalize to -1..1 (max lock is roughly 7.8 rad / ~450 degrees)
        double steering = Math.Clamp(steeringAngle / 7.8, -1.0, 1.0);

        return new InputsData
        {
            Throttle = ts.GetFloat("Throttle"),
            Brake = ts.GetFloat("Brake"),
            Clutch = ts.GetFloat("Clutch"),
            Steering = steering,
            Gear = ts.GetInt("Gear"),
            Speed = ts.GetFloat("Speed") * 3.6  // m/s → km/h
        };
    }

    private InputsData GetMockInputsData()
    {
        double throttle = Math.Max(0, Math.Sin(_elapsed * 1.2));
        double brake = Math.Max(0, Math.Sin(_elapsed * 1.2 + Math.PI));
        double steering = Math.Sin(_elapsed * 0.7);
        double clutch = Math.Max(0, Math.Sin(_elapsed * 0.3));

        int gear = ((int)(_elapsed / 3.0) % 8) + 1;
        if (gear > 6) gear = 6 - (gear - 6);

        double speed = 60 + throttle * 200 + _random.NextDouble() * 5;

        return new InputsData
        {
            Throttle = throttle,
            Brake = brake,
            Steering = steering,
            Clutch = clutch,
            Gear = gear,
            Speed = speed
        };
    }
}
