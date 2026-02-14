using RaceOverlay.Core.Providers;

namespace RaceOverlay.Providers.rFactor2;

public class rFactor2Provider : IGameProvider
{
    private readonly rFactor2DataService _dataService;

    public string GameId => "rFactor2";
    public string DisplayName => "rFactor 2 / Le Mans Ultimate";

    public event EventHandler<TelemetryData>? DataReceived;

    public rFactor2Provider(rFactor2DataService dataService)
    {
        _dataService = dataService;
        _dataService.TelemetryUpdated += OnTelemetryTick;
    }

    public bool IsGameRunning() => _dataService.IsConnected;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _dataService.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _dataService.Stop();
        return Task.CompletedTask;
    }

    private void OnTelemetryTick()
    {
        if (!_dataService.IsConnected) return;

        try
        {
            var data = new TelemetryData
            {
                Speed = _dataService.GetFloat("Speed") * 3.6f, // Convert m/s to km/h
                Rpm = _dataService.GetFloat("RPM"),
                Gear = _dataService.GetInt("Gear"),
                Throttle = _dataService.GetFloat("Throttle"),
                Brake = _dataService.GetFloat("Brake"),
                Clutch = _dataService.GetFloat("Clutch"),
                CurrentLapTime = TimeSpan.FromSeconds(_dataService.GetLapTime("CurrentLapTime")),
                LastLapTime = _dataService.GetLapTime("LastLapTime") > 0
                    ? TimeSpan.FromSeconds(_dataService.GetLapTime("LastLapTime"))
                    : null,
                BestLapTime = _dataService.GetLapTime("BestLapTime") > 0
                    ? TimeSpan.FromSeconds(_dataService.GetLapTime("BestLapTime"))
                    : null,
                LapNumber = _dataService.GetInt("Lap"),
                TrackName = _dataService.TrackName,
                CarName = _dataService.GetDriverInfo(_dataService.PlayerCarIdx)?.CarScreenNameShort
            };

            DataReceived?.Invoke(this, data);
        }
        catch
        {
            // Silently handle telemetry read errors
        }
    }
}
