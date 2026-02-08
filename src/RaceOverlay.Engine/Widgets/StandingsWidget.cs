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
    public bool ShowCarNumber { get; set; } = true;
    public bool ShowPositionsGained { get; set; } = true;
    public bool ShowLicense { get; set; } = true;
    public bool ShowIRating { get; set; } = true;
    public bool ShowCarBrand { get; set; } = true;
    public bool ShowInterval { get; set; } = true;
    public bool ShowGap { get; set; } = true;
    public bool ShowLastLapTime { get; set; } = true;
    public bool ShowDelta { get; set; } = true;
    public bool ShowPitStatus { get; set; } = true;
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

        // (name, class, classColor, carNumber, startingPos, license, licenseColor, iRating, carBrand)
        var driverData = new (string name, string cls, string clsColor, string carNum, int startPos,
            string license, string licColor, int iRating, string carBrand)[]
        {
            ("M. Verstappen", "GTE", "#3B82F6", "1",   3, "A 4.99", "#0153DB", 5200, "Porsche 911 GT3 R"),
            ("L. Hamilton",   "GTE", "#22C55E", "44",  1, "A 4.65", "#0153DB", 4800, "Mercedes AMG GT3"),
            ("C. Leclerc",    "GTE", "#EF4444", "16",  5, "A 3.99", "#0153DB", 4600, "Ferrari 296 GT3"),
            ("You",           "GT3", "#F97316", "88",  6, "B 3.21", "#00C12B", 3200, "McLaren 720S GT3"),
            ("L. Norris",     "GT3", "#F97316", "4",   2, "B 2.85", "#00C12B", 2900, "McLaren 720S GT3"),
            ("C. Sainz",      "GTE", "#EF4444", "55",  7, "A 4.12", "#0153DB", 4400, "Ferrari 296 GT3"),
            ("O. Piastri",    "GT3", "#F97316", "81",  9, "B 3.50", "#00C12B", 3100, "McLaren 720S GT3"),
            ("G. Russell",    "GTE", "#22C55E", "63",  4, "A 4.30", "#0153DB", 4500, "Mercedes AMG GT3"),
            ("F. Alonso",     "GT3", "#14B8A6", "14",  8, "C 3.75", "#FEEC04", 2200, "Aston Martin GT3"),
            ("P. Gasly",      "GT3", "#EC4899", "10", 12, "C 2.99", "#FEEC04", 2000, "Alpine A110 GT4"),
            ("Y. Tsunoda",    "GT3", "#3B82F6", "22", 10, "D 2.50", "#FC8A27", 1800, "Red Bull GT3"),
            ("A. Albon",      "GT3", "#6366F1", "23", 11, "D 1.75", "#FC8A27", 1500, "Williams GT3"),
        };

        double baseLapTime = 92.456;

        for (int i = 0; i < driverData.Length; i++)
        {
            var d = driverData[i];
            double gap = i == 0 ? 0.0 : 1.0 + i * 1.1 + _random.NextDouble() * 0.5;
            double bestLap = baseLapTime + gap * 0.3 + (_random.NextDouble() - 0.5) * 0.8;
            double lastLap = baseLapTime + gap * 0.25 + (_random.NextDouble() - 0.3) * 1.2;
            int posGained = d.startPos - (i + 1);
            int iRatingGain = _random.Next(-50, 51);

            _drivers.Add(new StandingDriver
            {
                Position = i + 1,
                DriverName = d.name,
                VehicleClass = d.cls,
                ClassColor = d.clsColor,
                CarNumber = d.carNum,
                StartingPosition = d.startPos,
                PositionsGained = posGained,
                LicenseClass = d.license,
                LicenseColor = d.licColor,
                IRating = d.iRating,
                IRatingGain = iRatingGain,
                CarBrand = d.carBrand,
                BestLapTime = bestLap,
                GapToLeader = gap,
                Interval = 0,
                LastLapTime = lastLap,
                Delta = 0,
                IsPlayer = d.name == "You",
                IsInPit = _random.Next(12) == 0
            });
        }

        // Compute interval and delta
        RecomputeDerivedFields();
    }

    private void RecomputeDerivedFields()
    {
        var player = _drivers.FirstOrDefault(d => d.IsPlayer);
        double playerLastLap = player?.LastLapTime ?? 0;

        for (int i = 0; i < _drivers.Count; i++)
        {
            var driver = _drivers[i];

            // Interval: gap to car directly ahead
            if (i == 0)
                driver.Interval = 0;
            else
                driver.Interval = driver.GapToLeader - _drivers[i - 1].GapToLeader;

            // Delta: relative to player's last lap
            if (driver.IsPlayer)
                driver.Delta = 0;
            else
                driver.Delta = driver.LastLapTime - playerLastLap;
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

                    // Slight drift in last lap time
                    driver.LastLapTime += (_random.NextDouble() - 0.5) * 0.08;

                    // Slight drift in gap (leader stays at 0)
                    if (driver.Position > 1)
                    {
                        driver.GapToLeader += (_random.NextDouble() - 0.5) * 0.15;
                        if (driver.GapToLeader < 0.1) driver.GapToLeader = 0.1;
                    }

                    // Slight drift in iRating gain
                    driver.IRatingGain += _random.Next(-2, 3);
                    if (driver.IRatingGain > 80) driver.IRatingGain = 80;
                    if (driver.IRatingGain < -80) driver.IRatingGain = -80;

                    // Rare pit status toggle
                    if (_random.Next(200) == 0)
                        driver.IsInPit = !driver.IsInPit;
                }

                // Recompute interval and delta each tick
                RecomputeDerivedFields();

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
