using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

/// <summary>
/// Configuration implementation for the Lap Timer widget.
/// </summary>
public class LapTimerConfig : ILapTimerConfig
{
    public int UpdateIntervalMs { get; set; } = 50;
    public bool UseMockData { get; set; } = true;
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;
    public bool ShowDeltaToBest { get; set; } = true;
    public bool ShowLastLap { get; set; } = true;
    public bool ShowBestLap { get; set; } = true;
    public bool ShowDeltaLastBest { get; set; } = true;
}

/// <summary>
/// Lap Timer widget that displays the player's current, last, and best lap times with deltas.
/// Mock data simulates a driver doing laps with realistic timing.
/// </summary>
public class LapTimerWidget : IWidget
{
    private LapTimerConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private readonly Random _random = new();

    private double _currentLapTime;
    private double _lastLapTime;
    private double _bestLapTime;
    private int _currentLap;
    private bool _isOutLap;

    // Mock lap timing — target lap length varies per lap
    private double _targetLapTime;

    public event Action? DataUpdated;

    public string WidgetId => "lap-timer";
    public string DisplayName => "Lap Timer";
    public string Description => "Displays current, last, and best lap times with delta comparisons.";
    public IWidgetConfiguration Configuration => _configuration;

    public int TotalLaps { get; private set; } = 24;

    public LapTimerWidget()
    {
        _configuration = new LapTimerConfig();
    }

    public void UpdateConfiguration(IWidgetConfiguration configuration)
    {
        if (configuration is LapTimerConfig config)
        {
            _configuration = config;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        InitializeMockData();

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
        // Simulate starting mid-race on an out lap
        _currentLap = 1;
        _currentLapTime = 0;
        _lastLapTime = 0;
        _bestLapTime = 0;
        _isOutLap = true;
        _targetLapTime = RandomLapTarget();
    }

    private double RandomLapTarget()
    {
        // Random lap time between 88 and 96 seconds
        return 88.0 + _random.NextDouble() * 8.0;
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                double tickSeconds = _configuration.UpdateIntervalMs / 1000.0;
                _currentLapTime += tickSeconds;

                // Check if the lap is "complete"
                if (_currentLapTime >= _targetLapTime)
                {
                    CompleteLap();
                }

                DataUpdated?.Invoke();

                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void CompleteLap()
    {
        double completedTime = _currentLapTime;

        if (!_isOutLap)
        {
            _lastLapTime = completedTime;

            if (_bestLapTime <= 0 || completedTime < _bestLapTime)
            {
                _bestLapTime = completedTime;
            }
        }

        _currentLap++;
        _currentLapTime = 0;
        _isOutLap = false;
        _targetLapTime = RandomLapTarget();
    }

    public LapTimerData GetLapTimerData()
    {
        double deltaToBest = 0;
        if (_bestLapTime > 0 && !_isOutLap)
        {
            // Project delta: how much slower/faster we are compared to best at this point in the lap
            deltaToBest = _currentLapTime - (_bestLapTime * (_currentLapTime / _targetLapTime));
        }

        double deltaLastBest = 0;
        if (_lastLapTime > 0 && _bestLapTime > 0)
        {
            deltaLastBest = _lastLapTime - _bestLapTime;
        }

        return new LapTimerData
        {
            CurrentLapTime = _currentLapTime,
            LastLapTime = _lastLapTime,
            BestLapTime = _bestLapTime,
            DeltaToBest = deltaToBest,
            DeltaLastBest = deltaLastBest,
            CurrentLap = _currentLap,
            TotalLaps = TotalLaps,
            IsOutLap = _isOutLap
        };
    }
}
