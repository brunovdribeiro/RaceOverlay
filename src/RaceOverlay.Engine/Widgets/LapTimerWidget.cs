using RaceOverlay.Core.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

public class LapTimerConfig : ILapTimerConfig
{
    public int UpdateIntervalMs { get; set; } = 50;
    public bool UseMockData { get; set; } = false;
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;
    public bool ShowDeltaToBest { get; set; } = true;
    public bool ShowLastLap { get; set; } = true;
    public bool ShowBestLap { get; set; } = true;
    public bool ShowDeltaLastBest { get; set; } = true;
}

public class LapTimerWidget : IWidget
{
    private LapTimerConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private readonly ILiveTelemetryService? _telemetryService;
    private readonly Random _random = new();

    private double _currentLapTime;
    private double _lastLapTime;
    private double _bestLapTime;
    private int _currentLap;
    private bool _isOutLap;

    // Mock lap timing
    private double _targetLapTime;

    public event Action? DataUpdated;

    public string WidgetId => "lap-timer";
    public string DisplayName => "Lap Timer";
    public string Description => "Displays current, last, and best lap times with delta comparisons.";
    public IWidgetConfiguration Configuration => _configuration;

    public int TotalLaps { get; private set; } = 24;

    private bool UseLiveData => !_configuration.UseMockData
                                && _telemetryService?.IsConnected == true;

    public LapTimerWidget(ILiveTelemetryService? telemetryService = null)
    {
        _configuration = new LapTimerConfig();
        _telemetryService = telemetryService;
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
        _currentLap = 1;
        _currentLapTime = 0;
        _lastLapTime = 0;
        _bestLapTime = 0;
        _isOutLap = true;
        _targetLapTime = RandomLapTarget();
    }

    private double RandomLapTarget()
    {
        return 88.0 + _random.NextDouble() * 8.0;
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!UseLiveData)
                {
                    double tickSeconds = _configuration.UpdateIntervalMs / 1000.0;
                    _currentLapTime += tickSeconds;

                    if (_currentLapTime >= _targetLapTime)
                    {
                        CompleteMockLap();
                    }
                }

                DataUpdated?.Invoke();

                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void CompleteMockLap()
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
        if (UseLiveData)
        {
            return GetLiveLapTimerData();
        }

        return GetMockLapTimerData();
    }

    private LapTimerData GetLiveLapTimerData()
    {
        var ts = _telemetryService!;

        float currentLapTime = ts.GetFloat("LapCurrentLapTime");
        float lastLapTime = ts.GetFloat("LapLastLapTime");
        float bestLapTime = ts.GetFloat("LapBestLapTime");
        float deltaToBest = ts.GetFloat("LapDeltaToBestLap");
        int currentLap = ts.GetInt("Lap");
        bool onPitRoad = ts.GetBool("OnPitRoad");

        int totalLaps = ts.SessionLaps;

        double deltaLastBest = 0;
        if (lastLapTime > 0 && bestLapTime > 0)
        {
            deltaLastBest = lastLapTime - bestLapTime;
        }

        bool isOutLap = currentLapTime <= 0 || onPitRoad;

        return new LapTimerData
        {
            CurrentLapTime = currentLapTime,
            LastLapTime = lastLapTime,
            BestLapTime = bestLapTime,
            DeltaToBest = deltaToBest,
            DeltaLastBest = deltaLastBest,
            CurrentLap = currentLap,
            TotalLaps = totalLaps,
            IsOutLap = isOutLap
        };
    }

    private LapTimerData GetMockLapTimerData()
    {
        double deltaToBest = 0;
        if (_bestLapTime > 0 && !_isOutLap)
        {
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
