using RaceOverlay.Core.Providers;
using RaceOverlay.Core.Services;

namespace RaceOverlay.Providers.iRacing;

public class IRacingProvider : IGameProvider
{
    private readonly IRacingDataService _dataService;

    public string GameId => "iRacing";
    public string DisplayName => "iRacing";
    public ILiveTelemetryService TelemetryService => _dataService;

    public event EventHandler<TelemetryData>? DataReceived;

    public IRacingProvider(IRacingDataService dataService)
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
                Speed = _dataService.GetFloat("Speed") * 3.6f,
                Rpm = _dataService.GetFloat("RPM"),
                Gear = _dataService.GetInt("Gear"),
                Throttle = _dataService.GetFloat("Throttle"),
                Brake = _dataService.GetFloat("Brake"),
                Clutch = _dataService.GetFloat("Clutch"),
                CurrentLapTime = TimeSpan.FromSeconds(_dataService.GetFloat("LapCurrentLapTime")),
                LastLapTime = TimeSpan.FromSeconds(_dataService.GetFloat("LapLastLapTime")),
                BestLapTime = TimeSpan.FromSeconds(_dataService.GetFloat("LapBestLapTime")),
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
