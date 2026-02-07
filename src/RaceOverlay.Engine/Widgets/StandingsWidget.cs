using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

/// <summary>
/// Configuration implementation for the Standings widget.
/// </summary>
public class StandingsConfig : IStandingsConfig
{
    public int UpdateIntervalMs { get; set; } = 500;
    public bool UseMockData { get; set; } = true;
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;
    public bool ShowClassColor { get; set; } = true;
    public bool ShowBestLapTime { get; set; } = true;
    public bool ShowGap { get; set; } = true;
    public int MaxDrivers { get; set; } = 20;
}

/// <summary>
/// Standings widget that displays a full race leaderboard sorted by overall position.
/// Shows all drivers with gaps, class colors, and highlights the player's row.
/// </summary>
public class StandingsWidget : IWidget
{
    private StandingsConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private List<StandingDriver> _drivers = new();
    private readonly Random _random = new();

    public event Action? DataUpdated;

    public string WidgetId => "standings";
    public string DisplayName => "Standings";
    public string Description => "Full race leaderboard showing all drivers sorted by position with gaps, class colors, and player highlighting.";
    public IWidgetConfiguration Configuration => _configuration;

    public int CurrentLap { get; private set; } = 12;
    public int TotalLaps { get; private set; } = 24;

    public StandingsWidget()
    {
        _configuration = new StandingsConfig();
    }

    public void UpdateConfiguration(IWidgetConfiguration configuration)
    {
        if (configuration is StandingsConfig config)
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
            ("M. Verstappen", "RBR", "#3B82F6"),
            ("L. Hamilton",   "MER", "#22C55E"),
            ("C. Leclerc",    "FER", "#EF4444"),
            ("You",           "MCL", "#F97316"),
            ("L. Norris",     "MCL", "#F97316"),
            ("C. Sainz",      "FER", "#EF4444"),
            ("O. Piastri",    "MCL", "#F97316"),
            ("G. Russell",    "MER", "#22C55E"),
            ("F. Alonso",     "AMR", "#14B8A6"),
            ("P. Gasly",      "ALP", "#EC4899"),
            ("Y. Tsunoda",    "RBR", "#3B82F6"),
            ("A. Albon",      "WIL", "#6366F1"),
        };

        double leaderLapTime = 92.456;

        for (int i = 0; i < driverData.Length; i++)
        {
            var (name, team, color) = driverData[i];
            double gap = i == 0 ? 0.0 : 1.0 + i * 1.1 + _random.NextDouble() * 0.5;
            double bestLap = leaderLapTime + gap * 0.3 + (_random.NextDouble() - 0.5) * 0.8;

            _drivers.Add(new StandingDriver
            {
                Position = i + 1,
                DriverName = name,
                VehicleClass = team,
                ClassColor = color,
                BestLapTime = bestLap,
                GapToLeader = gap,
                IsPlayer = name == "You",
                IsInPit = _random.Next(12) == 0
            });
        }
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var driver in _drivers)
                {
                    // Slight drift in best lap time
                    driver.BestLapTime += (_random.NextDouble() - 0.5) * 0.05;

                    // Slight drift in gap (leader stays at 0)
                    if (driver.Position > 1)
                    {
                        driver.GapToLeader += (_random.NextDouble() - 0.5) * 0.15;
                        if (driver.GapToLeader < 0.1) driver.GapToLeader = 0.1;
                    }

                    // Rare pit status toggle
                    if (_random.Next(200) == 0)
                        driver.IsInPit = !driver.IsInPit;
                }

                DataUpdated?.Invoke();

                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public IReadOnlyList<StandingDriver> GetStandings() => _drivers.AsReadOnly();
}
