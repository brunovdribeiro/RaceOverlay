using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

public class InputTraceConfig : IInputTraceConfig
{
    public int UpdateIntervalMs { get; set; } = 16;
    public bool UseMockData { get; set; } = true;
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;
    public string ThrottleColor { get; set; } = "#22C55E";
    public string BrakeColor { get; set; } = "#EF4444";
    public int HistorySeconds { get; set; } = 10;
}

public class InputTraceWidget : IWidget
{
    private InputTraceConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;

    // Mock state
    private double _elapsed;

    // Circular buffer
    private readonly Queue<InputTracePoint> _history = new();
    private readonly object _lock = new();

    public event Action? DataUpdated;

    public string WidgetId => "input-trace";
    public string DisplayName => "Input Trace";
    public string Description => "Scrolling line chart of throttle and brake inputs over time.";
    public IWidgetConfiguration Configuration => _configuration;

    public InputTraceWidget()
    {
        _configuration = new InputTraceConfig();
    }

    public void UpdateConfiguration(IWidgetConfiguration configuration)
    {
        if (configuration is InputTraceConfig config)
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

                // Same sinusoidal mock data as InputsWidget
                double throttle = Math.Max(0, Math.Sin(_elapsed * 1.2));
                double brake = Math.Max(0, Math.Sin(_elapsed * 1.2 + Math.PI));

                int maxPoints = _configuration.HistorySeconds * 1000 / _configuration.UpdateIntervalMs;

                lock (_lock)
                {
                    _history.Enqueue(new InputTracePoint(throttle, brake));
                    while (_history.Count > maxPoints)
                    {
                        _history.Dequeue();
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

    public IReadOnlyList<InputTracePoint> GetTraceHistory()
    {
        lock (_lock)
        {
            return _history.ToArray();
        }
    }
}
