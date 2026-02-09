using RaceOverlay.Core.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

public class RelativeOverlayConfig : IRelativeOverlayConfig
{
    public int DriversAhead { get; set; } = 3;
    public int DriversBehind { get; set; } = 3;
    public bool UseMockData { get; set; } = false;
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

public class RelativeOverlay : IWidget
{
    private RelativeOverlayConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private List<RelativeDriver> _relativeDrivers = new();
    private readonly Random _random = new();
    private readonly ILiveTelemetryService? _telemetryService;

    public event Action? DataUpdated;

    public string WidgetId => "relative-overlay";
    public string DisplayName => "Relative Overlay";
    public string Description => "Shows drivers around you with live lap times, stint information, and Elo ratings. Perfect for measuring pace and making smarter on-track decisions.";
    public IWidgetConfiguration Configuration => _configuration;

    private bool UseLiveData => !_configuration.UseMockData
                                && _telemetryService?.IsConnected == true;

    public RelativeOverlay(ILiveTelemetryService? telemetryService = null)
    {
        _configuration = new RelativeOverlayConfig();
        _telemetryService = telemetryService;
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
        _relativeDrivers.Clear();
    }

    private void InitializeMockData()
    {
        _relativeDrivers.Clear();

        var driverNames = new[] { "A. Rivera", "J. Lindqvist", "T. Nakamura", "E. Bergmann", "D. Okafor", "M. Janssen", "R. Petrov", "K. Tanaka", "S. Fischer", "L. Moreno" };

        double playerTrackDistance = 5000;

        for (int i = 0; i < 3; i++)
        {
            _relativeDrivers.Add(CreateMockDriver(
                position: i + 1,
                number: (i + 5).ToString(),
                name: driverNames[i],
                trackDistance: playerTrackDistance + (500 + (i * 100)),
                relativePosition: -1
            ));
        }

        _relativeDrivers.Add(CreateMockDriver(
            position: 6,
            number: "12",
            name: "You",
            trackDistance: playerTrackDistance,
            relativePosition: 0
        ));

        for (int i = 0; i < 3; i++)
        {
            _relativeDrivers.Add(CreateMockDriver(
                position: 7 + i,
                number: (13 + i).ToString(),
                name: driverNames[3 + i],
                trackDistance: playerTrackDistance - (200 + (i * 150)),
                relativePosition: 1
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

        double bestLapTime = 90 + _random.NextDouble() * 30;
        double currentLapTime = bestLapTime + _random.NextDouble() * 5;
        double gapToNext = (_random.NextDouble() - 0.5) * 10;

        return new RelativeDriver
        {
            Position = position,
            Number = number,
            DriverName = name,
            VehicleClass = vehicleClasses[classIndex],
            ClassColor = classColors[classIndex],
            EloRating = 1600 + _random.Next(800),
            EloGrade = eloGrades[eloIndex],
            EloGradeColor = eloGradeColors[eloIndex],
            CurrentLapTime = currentLapTime,
            BestLapTime = bestLapTime,
            DeltaFromBest = currentLapTime - bestLapTime,
            GapToNextDriver = gapToNext,
            StintLapsCompleted = _random.Next(5, 25),
            StintLapsTotal = 30,
            StintTime = $"{_random.Next(10, 45):D2}:{_random.Next(0, 60):D2}",
            IsInPit = _random.Next(10) == 0,
            StatusFlag = _random.Next(20) == 0 ? "OUT" : null,
            HasDamage = _random.Next(15) == 0,
            TrackDistanceMeters = trackDistance,
            RelativePosition = relativePosition
        };
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (UseLiveData)
                {
                    UpdateLiveRelative();
                }
                else
                {
                    UpdateMockRelative();
                }

                DataUpdated?.Invoke();

                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void UpdateMockRelative()
    {
        foreach (var driver in _relativeDrivers)
        {
            driver.CurrentLapTime += ((_random.NextDouble() - 0.5) * 0.5);
            driver.DeltaFromBest = driver.CurrentLapTime - driver.BestLapTime;
            driver.GapToNextDriver += ((_random.NextDouble() - 0.5) * 0.2);
        }
    }

    private void UpdateLiveRelative()
    {
        var ts = _telemetryService!;
        int playerCarIdx = ts.PlayerCarIdx;
        int driverCount = ts.DriverCount;
        float trackLengthM = ts.TrackLengthKm * 1000;

        float playerLapDistPct = ts.GetFloat("CarIdxLapDistPct", playerCarIdx);
        float playerEstTime = ts.GetFloat("CarIdxEstTime", playerCarIdx);

        // Gather all active drivers with their track position
        var allDrivers = new List<(int carIdx, float lapDistPct, float relDistPct, DriverSessionInfo info)>();

        for (int i = 0; i < Math.Min(driverCount, 64); i++)
        {
            var driverInfo = ts.GetDriverInfo(i);
            if (driverInfo == null || driverInfo.IsSpectator) continue;

            int position = ts.GetInt("CarIdxPosition", i);
            if (position <= 0 && i != playerCarIdx) continue;

            float lapDistPct = ts.GetFloat("CarIdxLapDistPct", i);
            if (lapDistPct < 0) continue;

            // Relative distance: how far ahead (+) or behind (-) on track, wrapping at 0.5
            float relDist = lapDistPct - playerLapDistPct;
            if (relDist > 0.5f) relDist -= 1.0f;
            if (relDist < -0.5f) relDist += 1.0f;

            allDrivers.Add((i, lapDistPct, relDist, driverInfo));
        }

        // Sort by relative distance (most ahead first, most behind last)
        allDrivers.Sort((a, b) => b.relDistPct.CompareTo(a.relDistPct));

        // Find player index in sorted list
        int playerIndex = allDrivers.FindIndex(d => d.carIdx == playerCarIdx);

        // Select drivers ahead and behind
        var selected = new List<(int carIdx, float lapDistPct, float relDistPct, DriverSessionInfo info)>();

        // Drivers ahead (closest first)
        int aheadCount = 0;
        for (int i = playerIndex - 1; i >= 0 && aheadCount < _configuration.DriversAhead; i--)
        {
            selected.Insert(0, allDrivers[i]);
            aheadCount++;
        }
        // Wrap around if needed
        if (aheadCount < _configuration.DriversAhead)
        {
            for (int i = allDrivers.Count - 1; i > playerIndex && aheadCount < _configuration.DriversAhead; i--)
            {
                selected.Insert(0, allDrivers[i]);
                aheadCount++;
            }
        }

        // Add player
        if (playerIndex >= 0)
            selected.Add(allDrivers[playerIndex]);

        // Drivers behind (closest first)
        int behindCount = 0;
        for (int i = playerIndex + 1; i < allDrivers.Count && behindCount < _configuration.DriversBehind; i++)
        {
            selected.Add(allDrivers[i]);
            behindCount++;
        }
        // Wrap around if needed
        if (behindCount < _configuration.DriversBehind)
        {
            for (int i = 0; i < playerIndex && behindCount < _configuration.DriversBehind; i++)
            {
                selected.Add(allDrivers[i]);
                behindCount++;
            }
        }

        // Resize _relativeDrivers
        while (_relativeDrivers.Count < selected.Count)
            _relativeDrivers.Add(new RelativeDriver());
        while (_relativeDrivers.Count > selected.Count)
            _relativeDrivers.RemoveAt(_relativeDrivers.Count - 1);

        for (int i = 0; i < selected.Count; i++)
        {
            var (carIdx, lapDistPct, relDistPct, info) = selected[i];
            var driver = _relativeDrivers[i];

            driver.Position = ts.GetInt("CarIdxPosition", carIdx);
            driver.Number = info.CarNumber;
            driver.DriverName = info.UserName;
            driver.VehicleClass = info.CarClassShortName;
            driver.ClassColor = $"#{info.CarClassColor:X6}";
            driver.IsInPit = ts.GetBool("CarIdxOnPitRoad", carIdx);

            float bestLapTime = ts.GetFloat("CarIdxBestLapTime", carIdx);
            float lastLapTime = ts.GetFloat("CarIdxLastLapTime", carIdx);
            driver.BestLapTime = bestLapTime > 0 ? bestLapTime : 0;
            driver.CurrentLapTime = lastLapTime > 0 ? lastLapTime : 0;
            driver.DeltaFromBest = bestLapTime > 0 && lastLapTime > 0 ? lastLapTime - bestLapTime : 0;

            // Elo/iRating mapping
            driver.EloRating = info.IRating;
            (driver.EloGrade, driver.EloGradeColor) = GetIRatingGrade(info.IRating);

            // Gap: use EstTime difference from player
            float driverEstTime = ts.GetFloat("CarIdxEstTime", carIdx);
            driver.GapToNextDriver = driverEstTime - playerEstTime;

            driver.TrackDistanceMeters = lapDistPct * trackLengthM;

            // Relative position: -1 ahead, 0 player, 1 behind
            if (carIdx == playerCarIdx)
                driver.RelativePosition = 0;
            else if (relDistPct > 0)
                driver.RelativePosition = -1; // ahead
            else
                driver.RelativePosition = 1; // behind
        }
    }

    private static (string grade, string color) GetIRatingGrade(int iRating) => iRating switch
    {
        >= 4000 => ("A+", "#7C3AED"),
        >= 3000 => ("A", "#3B82F6"),
        >= 2000 => ("B", "#22C55E"),
        >= 1500 => ("C", "#F59E0B"),
        >= 1000 => ("D", "#F97316"),
        _ => ("R", "#EF4444")
    };

    public IReadOnlyList<RelativeDriver> GetRelativeDrivers() => _relativeDrivers.AsReadOnly();
}
