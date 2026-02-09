using RaceOverlay.Core.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

public class StandingsConfig : IStandingsConfig
{
    public int UpdateIntervalMs { get; set; } = 500;
    public bool UseMockData { get; set; } = false;
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

public class StandingsWidget : IWidget
{
    private StandingsConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private List<StandingDriver> _drivers = new();
    private readonly Random _random = new();
    private readonly ILiveTelemetryService? _telemetryService;

    public event Action? DataUpdated;

    public string WidgetId => "standings";
    public string DisplayName => "Standings";
    public string Description => "Full race leaderboard showing all drivers sorted by position with gaps, class colors, and player highlighting.";
    public IWidgetConfiguration Configuration => _configuration;

    public int CurrentLap { get; private set; } = 12;
    public int TotalLaps { get; private set; } = 24;

    private bool UseLiveData => !_configuration.UseMockData
                                && _telemetryService?.IsConnected == true;

    public StandingsWidget(ILiveTelemetryService? telemetryService = null)
    {
        _configuration = new StandingsConfig();
        _telemetryService = telemetryService;
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
        _drivers.Clear();
    }

    private void InitializeMockData()
    {
        _drivers.Clear();

        var driverData = new (string name, string cls, string clsColor, string carNum, int startPos,
            string license, string licColor, int iRating, string carBrand)[]
        {
            ("A. Rivera",     "GTE", "#3B82F6", "1",   3, "A 4.99", "#0153DB", 5200, "Porsche 911 GT3 R"),
            ("J. Lindqvist",  "GTE", "#22C55E", "44",  1, "A 4.65", "#0153DB", 4800, "Mercedes AMG GT3"),
            ("T. Nakamura",   "GTE", "#EF4444", "16",  5, "A 3.99", "#0153DB", 4600, "Ferrari 296 GT3"),
            ("You",           "GT3", "#F97316", "88",  6, "B 3.21", "#00C12B", 3200, "McLaren 720S GT3"),
            ("E. Bergmann",   "GT3", "#F97316", "4",   2, "B 2.85", "#00C12B", 2900, "McLaren 720S GT3"),
            ("D. Okafor",     "GTE", "#EF4444", "55",  7, "A 4.12", "#0153DB", 4400, "Ferrari 296 GT3"),
            ("M. Janssen",    "GT3", "#F97316", "81",  9, "B 3.50", "#00C12B", 3100, "McLaren 720S GT3"),
            ("R. Petrov",     "GTE", "#22C55E", "63",  4, "A 4.30", "#0153DB", 4500, "Mercedes AMG GT3"),
            ("K. Tanaka",     "GT3", "#14B8A6", "14",  8, "C 3.75", "#FEEC04", 2200, "Aston Martin GT3"),
            ("S. Fischer",    "GT3", "#EC4899", "10", 12, "C 2.99", "#FEEC04", 2000, "Alpine A110 GT4"),
            ("L. Moreno",     "GT3", "#3B82F6", "22", 10, "D 2.50", "#FC8A27", 1800, "Red Bull GT3"),
            ("H. Kowalski",   "GT3", "#6366F1", "23", 11, "D 1.75", "#FC8A27", 1500, "Williams GT3"),
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

        RecomputeDerivedFields();
    }

    private void RecomputeDerivedFields()
    {
        var player = _drivers.FirstOrDefault(d => d.IsPlayer);
        double playerLastLap = player?.LastLapTime ?? 0;

        for (int i = 0; i < _drivers.Count; i++)
        {
            var driver = _drivers[i];

            if (i == 0)
                driver.Interval = 0;
            else
                driver.Interval = driver.GapToLeader - _drivers[i - 1].GapToLeader;

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
                if (UseLiveData)
                {
                    UpdateLiveStandings();
                }
                else
                {
                    UpdateMockStandings();
                }

                DataUpdated?.Invoke();

                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void UpdateMockStandings()
    {
        foreach (var driver in _drivers)
        {
            driver.BestLapTime += (_random.NextDouble() - 0.5) * 0.05;
            driver.LastLapTime += (_random.NextDouble() - 0.5) * 0.08;

            if (driver.Position > 1)
            {
                driver.GapToLeader += (_random.NextDouble() - 0.5) * 0.15;
                if (driver.GapToLeader < 0.1) driver.GapToLeader = 0.1;
            }

            driver.IRatingGain += _random.Next(-2, 3);
            if (driver.IRatingGain > 80) driver.IRatingGain = 80;
            if (driver.IRatingGain < -80) driver.IRatingGain = -80;

            if (_random.Next(200) == 0)
                driver.IsInPit = !driver.IsInPit;
        }

        RecomputeDerivedFields();
    }

    private void UpdateLiveStandings()
    {
        var ts = _telemetryService!;
        int playerCarIdx = ts.PlayerCarIdx;
        int driverCount = ts.DriverCount;

        CurrentLap = ts.GetInt("Lap");
        TotalLaps = ts.SessionLaps;

        // Build list of active drivers with positions
        var liveDrivers = new List<(int carIdx, int position, DriverSessionInfo info)>();

        for (int i = 0; i < Math.Min(driverCount, 64); i++)
        {
            var driverInfo = ts.GetDriverInfo(i);
            if (driverInfo == null || driverInfo.IsSpectator) continue;

            int position = ts.GetInt("CarIdxPosition", i);
            if (position <= 0) continue; // not on track / not classified

            liveDrivers.Add((i, position, driverInfo));
        }

        // Sort by position
        liveDrivers.Sort((a, b) => a.position.CompareTo(b.position));

        // Resize _drivers list to match
        while (_drivers.Count < liveDrivers.Count)
            _drivers.Add(new StandingDriver());
        while (_drivers.Count > liveDrivers.Count)
            _drivers.RemoveAt(_drivers.Count - 1);

        double leaderF2Time = 0;
        double playerLastLap = 0;

        for (int i = 0; i < liveDrivers.Count; i++)
        {
            var (carIdx, position, info) = liveDrivers[i];
            var driver = _drivers[i];

            driver.Position = position;
            driver.DriverName = info.UserName;
            driver.VehicleClass = info.CarClassShortName;
            driver.ClassColor = $"#{info.CarClassColor:X6}";
            driver.CarNumber = info.CarNumber;
            driver.LicenseClass = info.LicString;
            driver.LicenseColor = $"#{info.LicColor:X6}";
            driver.IRating = info.IRating;
            driver.CarBrand = info.CarScreenNameShort;
            driver.IsPlayer = carIdx == playerCarIdx;
            driver.IsInPit = ts.GetBool("CarIdxOnPitRoad", carIdx);

            float bestLapTime = ts.GetFloat("CarIdxBestLapTime", carIdx);
            float lastLapTime = ts.GetFloat("CarIdxLastLapTime", carIdx);
            float f2Time = ts.GetFloat("CarIdxF2Time", carIdx);

            driver.BestLapTime = bestLapTime > 0 ? bestLapTime : 0;
            driver.LastLapTime = lastLapTime > 0 ? lastLapTime : 0;

            // Gap to leader from F2Time
            if (i == 0)
            {
                leaderF2Time = f2Time;
                driver.GapToLeader = 0;
            }
            else
            {
                driver.GapToLeader = f2Time > 0 ? f2Time : 0;
            }

            // Interval to car ahead
            if (i == 0)
            {
                driver.Interval = 0;
            }
            else
            {
                float prevF2Time = ts.GetFloat("CarIdxF2Time", liveDrivers[i - 1].carIdx);
                driver.Interval = f2Time - prevF2Time;
            }

            if (driver.IsPlayer)
            {
                playerLastLap = driver.LastLapTime;
            }
        }

        // Compute delta relative to player
        foreach (var driver in _drivers)
        {
            if (driver.IsPlayer)
                driver.Delta = 0;
            else if (playerLastLap > 0 && driver.LastLapTime > 0)
                driver.Delta = driver.LastLapTime - playerLastLap;
            else
                driver.Delta = 0;
        }
    }

    public IReadOnlyList<StandingDriver> GetStandings() => _drivers.AsReadOnly();
}
