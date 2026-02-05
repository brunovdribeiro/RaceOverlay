using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

/// <summary>
/// Configuration implementation for the Relative Overlay widget.
/// </summary>
public class RelativeOverlayConfig : IRelativeOverlayConfig
{
    public int DriversAhead { get; set; } = 3;
    public int DriversBehind { get; set; } = 3;
    public bool UseMockData { get; set; } = true;
    public int UpdateIntervalMs { get; set; } = 500;
    public bool ShowPosition { get; set; } = true;
    public bool ShowClassColor { get; set; } = true;
    public bool ShowDriverName { get; set; } = true;
    public bool ShowRating { get; set; } = true;
    public bool ShowStint { get; set; } = true;
    public bool ShowLapTime { get; set; } = true;
    public bool ShowGap { get; set; } = true;
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;
}

/// <summary>
/// Relative Overlay widget that displays drivers relative to the player's position on track.
/// Shows drivers ahead and behind sorted by track distance with live lap times, stint info, and Elo ratings.
/// </summary>
public class RelativeOverlay : IWidget
{
    private RelativeOverlayConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private List<RelativeDriver> _relativeDrivers = new();
    private readonly Random _random = new();

    /// <summary>
    /// Fired at the end of each update loop tick so the overlay window knows when to refresh.
    /// </summary>
    public event Action? DataUpdated;

    public string WidgetId => "relative-overlay";
    public string DisplayName => "Relative Overlay";
    public string Description => "Shows drivers around you with live lap times, stint information, and Elo ratings. Perfect for measuring pace and making smarter on-track decisions.";
    public IWidgetConfiguration Configuration => _configuration;

    public RelativeOverlay()
    {
        _configuration = new RelativeOverlayConfig();
    }

    public void UpdateConfiguration(IWidgetConfiguration configuration)
    {
        if (configuration is RelativeOverlayConfig config)
        {
            _configuration = config;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Initialize mock data
        InitializeMockData();

        // Start the update loop
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
                // Expected when cancellation is requested
            }
        }

        _cancellationTokenSource?.Dispose();
        _relativeDrivers.Clear();
    }

    /// <summary>
    /// Initializes mock data for 7 drivers (3 ahead, player, 3 behind).
    /// </summary>
    private void InitializeMockData()
    {
        _relativeDrivers.Clear();

        // Define some mock driver names and numbers
        var driverNames = new[] { "Rogerio Silva", "Matthew Naylor", "Rick Zwieten", "Istvan Fodor", "Sindre Setsaas", "Alexandr Fescov", "Marius Rieck", "John Smith", "Lucas Costa", "Max Verstappen" };

        // Player position at track distance 0 (reference point)
        double playerTrackDistance = 5000;

        // Create drivers ahead of player
        for (int i = 0; i < 3; i++)
        {
            _relativeDrivers.Add(CreateMockDriver(
                position: i + 1,
                number: (i + 5).ToString(),
                name: driverNames[i],
                trackDistance: playerTrackDistance + (500 + (i * 100)),
                relativePosition: -1 // Ahead
            ));
        }

        // Add player
        _relativeDrivers.Add(CreateMockDriver(
            position: 6,
            number: "12",
            name: "You",
            trackDistance: playerTrackDistance,
            relativePosition: 0 // Player
        ));

        // Create drivers behind player
        for (int i = 0; i < 3; i++)
        {
            _relativeDrivers.Add(CreateMockDriver(
                position: 7 + i,
                number: (13 + i).ToString(),
                name: driverNames[3 + i],
                trackDistance: playerTrackDistance - (200 + (i * 150)),
                relativePosition: 1 // Behind
            ));
        }
    }

    private RelativeDriver CreateMockDriver(int position, string number, string name, double trackDistance, int relativePosition)
    {
        var vehicleClasses = new[] { "GTE", "GT3", "P2" };
        var classColors = new[] { "#D946EF", "#D97706", "#6B7280" };
        var eloGrades = new[] { "A", "B", "C" };
        var eloGradeColors = new[] { "#3B82F6", "#22C55E", "#6B7280" };

        int classIndex = _random.Next(vehicleClasses.Length);
        int eloIndex = _random.Next(eloGrades.Length);

        double bestLapTime = 90 + _random.NextDouble() * 30; // 90-120 seconds
        double currentLapTime = bestLapTime + _random.NextDouble() * 5; // Within 5 seconds of best
        double gapToNext = (_random.NextDouble() - 0.5) * 10; // -5.0 to +5.0 seconds

        return new RelativeDriver
        {
            Position = position,
            Number = number,
            DriverName = name,
            VehicleClass = vehicleClasses[classIndex],
            ClassColor = classColors[classIndex],
            EloRating = 1600 + _random.Next(800), // 1600-2400 range
            EloGrade = eloGrades[eloIndex],
            EloGradeColor = eloGradeColors[eloIndex],
            CurrentLapTime = currentLapTime,
            BestLapTime = bestLapTime,
            DeltaFromBest = currentLapTime - bestLapTime,
            GapToNextDriver = gapToNext,
            StintLapsCompleted = _random.Next(5, 25),
            StintLapsTotal = 30,
            StintTime = $"{_random.Next(10, 45):D2}:{_random.Next(0, 60):D2}",
            IsInPit = _random.Next(10) == 0, // 10% chance
            StatusFlag = _random.Next(20) == 0 ? "OUT" : null, // 5% chance
            HasDamage = _random.Next(15) == 0, // ~6% chance
            TrackDistanceMeters = trackDistance,
            RelativePosition = relativePosition
        };
    }

    /// <summary>
    /// Update loop that periodically refreshes driver data.
    /// </summary>
    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Simulate minor variations in lap times and gaps
                foreach (var driver in _relativeDrivers)
                {
                    // Slight variation in current lap time
                    driver.CurrentLapTime += ((_random.NextDouble() - 0.5) * 0.5);
                    driver.DeltaFromBest = driver.CurrentLapTime - driver.BestLapTime;

                    // Minor gap variation
                    driver.GapToNextDriver += ((_random.NextDouble() - 0.5) * 0.2);
                }

                DataUpdated?.Invoke();

                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping the widget
        }
    }

    /// <summary>
    /// Gets the current list of relative drivers (for binding to view).
    /// </summary>
    public IReadOnlyList<RelativeDriver> GetRelativeDrivers() => _relativeDrivers.AsReadOnly();
}
