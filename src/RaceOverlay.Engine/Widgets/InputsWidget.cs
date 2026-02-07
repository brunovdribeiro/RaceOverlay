using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

public class InputsConfig : IInputsConfig
{
    public int UpdateIntervalMs { get; set; } = 16;
    public bool UseMockData { get; set; } = true;
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

    // Mock state
    private double _elapsed;
    private readonly Random _random = new();

    public event Action? DataUpdated;

    public string WidgetId => "inputs";
    public string DisplayName => "Inputs";
    public string Description => "Visualizes throttle, brake, and steering telemetry in real time.";
    public IWidgetConfiguration Configuration => _configuration;

    public InputsWidget()
    {
        _configuration = new InputsConfig();
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
        // Sinusoidal mock data for development
        double throttle = Math.Max(0, Math.Sin(_elapsed * 1.2)) ;
        double brake = Math.Max(0, Math.Sin(_elapsed * 1.2 + Math.PI));
        double steering = Math.Sin(_elapsed * 0.7);
        double clutch = Math.Max(0, Math.Sin(_elapsed * 0.3));

        // Cycle through gears based on elapsed time
        int gear = ((int)(_elapsed / 3.0) % 8) + 1;
        if (gear > 6) gear = 6 - (gear - 6); // bounce 1-6-1

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
