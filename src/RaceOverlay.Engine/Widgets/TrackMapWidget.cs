using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

public class TrackMapConfig : ITrackMapConfig
{
    public int UpdateIntervalMs { get; set; } = 50;
    public bool UseMockData { get; set; } = true;
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;
    public bool ShowDriverNames { get; set; } = false;
    public bool ShowPitStatus { get; set; } = true;
}

public class TrackMapWidget : IWidget
{
    private TrackMapConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private readonly List<TrackMapDriver> _drivers = new();
    private readonly Random _random = new();
    private readonly double[] _driverSpeeds = new double[12];

    public event Action? DataUpdated;

    public string WidgetId => "track-map";
    public string DisplayName => "Track Map";
    public string Description => "Minimap showing the track outline with colored dots for each car's position.";
    public IWidgetConfiguration Configuration => _configuration;

    public int CurrentLap { get; private set; } = 12;
    public int TotalLaps { get; private set; } = 24;

    // Normalized track outline (0-1 range), roughly a D-shaped circuit
    public static readonly (double X, double Y)[] TrackOutline = new[]
    {
        (0.30, 0.05), (0.35, 0.04), (0.40, 0.03), (0.45, 0.03),
        (0.50, 0.03), (0.55, 0.03), (0.60, 0.04), (0.65, 0.05),
        (0.70, 0.07), (0.75, 0.10), (0.79, 0.14), (0.82, 0.18),
        (0.85, 0.23), (0.87, 0.28), (0.88, 0.33), (0.89, 0.38),
        (0.89, 0.43), (0.89, 0.48), (0.88, 0.53), (0.87, 0.58),
        (0.85, 0.63), (0.82, 0.68), (0.79, 0.72), (0.75, 0.76),
        (0.70, 0.80), (0.65, 0.83), (0.60, 0.85), (0.55, 0.87),
        (0.50, 0.88), (0.45, 0.88), (0.40, 0.87), (0.35, 0.85),
        (0.30, 0.83), (0.25, 0.80), (0.21, 0.76), (0.18, 0.72),
        (0.15, 0.68), (0.13, 0.63), (0.12, 0.58), (0.11, 0.53),
        (0.11, 0.48), (0.11, 0.43), (0.12, 0.38), (0.13, 0.33),
        (0.15, 0.28), (0.18, 0.23), (0.21, 0.18), (0.25, 0.14),
        (0.30, 0.10), (0.30, 0.05),
    };

    public TrackMapWidget()
    {
        _configuration = new TrackMapConfig();
    }

    public void UpdateConfiguration(IWidgetConfiguration configuration)
    {
        if (configuration is TrackMapConfig config)
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
        _drivers.Clear();
    }

    private void InitializeMockData()
    {
        _drivers.Clear();

        var driverData = new[]
        {
            ("M. Verstappen", "#3B82F6"),
            ("L. Hamilton",   "#22C55E"),
            ("C. Leclerc",    "#EF4444"),
            ("You",           "#F97316"),
            ("L. Norris",     "#F97316"),
            ("C. Sainz",      "#EF4444"),
            ("O. Piastri",    "#F97316"),
            ("G. Russell",    "#22C55E"),
            ("F. Alonso",     "#14B8A6"),
            ("P. Gasly",      "#EC4899"),
            ("Y. Tsunoda",    "#3B82F6"),
            ("A. Albon",      "#6366F1"),
        };

        double baseSpeed = 0.003; // progress per tick (~60 ticks/sec at 50ms)

        for (int i = 0; i < driverData.Length; i++)
        {
            var (name, color) = driverData[i];

            _drivers.Add(new TrackMapDriver
            {
                DriverName = name,
                ClassColor = color,
                TrackProgress = i * 0.07, // spread drivers around the track
                IsPlayer = name == "You",
                IsInPit = false
            });

            // Slightly different speed per driver
            _driverSpeeds[i] = baseSpeed + (_random.NextDouble() - 0.5) * 0.0006;
        }
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                for (int i = 0; i < _drivers.Count; i++)
                {
                    _drivers[i].TrackProgress += _driverSpeeds[i];

                    // Wrap at 1.0
                    if (_drivers[i].TrackProgress >= 1.0)
                        _drivers[i].TrackProgress -= 1.0;

                    // Rare pit status toggle
                    if (_random.Next(400) == 0)
                        _drivers[i].IsInPit = !_drivers[i].IsInPit;
                }

                DataUpdated?.Invoke();

                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public (IReadOnlyList<TrackMapDriver> Drivers, int CurrentLap, int TotalLaps) GetTrackMapData()
    {
        return (_drivers.AsReadOnly(), CurrentLap, TotalLaps);
    }

    public (double X, double Y)[] GetTrackOutline() => TrackOutline;
}
