namespace RaceOverlay.Core.Services;

/// <summary>
/// Demo telemetry service that provides simulated data when no game is running
/// </summary>
public class DemoTelemetryService : ILiveTelemetryService
{
    private volatile bool _isRunning;
    private CancellationTokenSource? _cts;
    private Task? _updateTask;
    private readonly Random _random = new();
    private double _currentSpeed = 0;
    private double _currentRpm = 1000;
    private int _currentGear = 1;
    private double _lapTime = 0;
    private double _lastLapTime = 87.543;
    private double _bestLapTime = 85.123;
    private int _currentLap = 3;

    public bool IsConnected => _isRunning;

    public event Action? TelemetryUpdated;
    public event Action? SessionInfoUpdated;
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public int DriverCount => 20; // Simulate 20 drivers
    public int PlayerCarIdx => 5; // Player is car index 5
    public string? TrackName => "Demo Circuit";
    public int TrackId => 0;
    public string? TrackConfigName => "Grand Prix";
    public float TrackLengthKm => 5.5f;
    public int SessionLaps => 25;

    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        _cts = new CancellationTokenSource();
        _updateTask = Task.Run(async () => await UpdateLoopAsync(_cts.Token), _cts.Token);

        OnConnected?.Invoke();
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _cts?.Cancel();
        _updateTask?.Wait(TimeSpan.FromSeconds(1));
        _updateTask = null;
        _cts?.Dispose();
        _cts = null;
        _isRunning = false;

        OnDisconnected?.Invoke();
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Simulate realistic racing telemetry
                UpdateSimulatedTelemetry();

                TelemetryUpdated?.Invoke();

                // ~60Hz update rate
                await Task.Delay(16, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void UpdateSimulatedTelemetry()
    {
        // Simulate acceleration and braking
        _currentSpeed += (_random.NextDouble() - 0.5) * 5;
        _currentSpeed = Math.Clamp(_currentSpeed, 0, 280);

        // Simulate RPM based on speed and gear
        _currentRpm = 2000 + (_currentSpeed / 280.0) * 6000 + _random.NextDouble() * 500;
        _currentRpm = Math.Clamp(_currentRpm, 800, 8500);

        // Simulate gear changes
        if (_currentSpeed > 40 * _currentGear && _currentGear < 6)
            _currentGear++;
        else if (_currentSpeed < 30 * (_currentGear - 1) && _currentGear > 1)
            _currentGear--;

        // Simulate lap progress
        _lapTime += 0.016; // 16ms per frame
        if (_lapTime > 90) // Complete a lap every ~90 seconds
        {
            _lastLapTime = _lapTime;
            _lapTime = 0;
            _currentLap++;

            if (_lastLapTime < _bestLapTime)
                _bestLapTime = _lastLapTime;
        }
    }

    public float GetFloat(string variableName)
    {
        return variableName switch
        {
            "Speed" => (float)_currentSpeed,
            "RPM" => (float)_currentRpm,
            "Throttle" => (float)(_currentSpeed > 100 ? 0.8 + _random.NextDouble() * 0.2 : _random.NextDouble()),
            "Brake" => (float)(_currentSpeed < 80 ? _random.NextDouble() * 0.3 : 0),
            "Clutch" => 0f,
            "Steering" => (float)(Math.Sin(DateTime.Now.Ticks / 10000000.0) * 0.3),
            "Fuel" => 45.5f,
            "FuelCapacity" => 100f,
            _ => 0f
        };
    }

    public int GetInt(string variableName)
    {
        return variableName switch
        {
            "Gear" => _currentGear,
            "Lap" => _currentLap,
            "Position" => 7,
            _ => 0
        };
    }

    public bool GetBool(string variableName)
    {
        return variableName switch
        {
            "InPits" => false,
            _ => false
        };
    }

    public float GetFloat(string variableName, int carIdx)
    {
        // Simulate other cars with slight variations
        var offset = carIdx * 0.1;
        return variableName switch
        {
            "Speed" => (float)(_currentSpeed + offset * 10),
            "RPM" => (float)(_currentRpm + offset * 100),
            _ => 0f
        };
    }

    public int GetInt(string variableName, int carIdx)
    {
        return variableName switch
        {
            "Position" => carIdx + 1,
            "Lap" => _currentLap + (carIdx < PlayerCarIdx ? 1 : 0),
            _ => 0
        };
    }

    public bool GetBool(string variableName, int carIdx)
    {
        return false;
    }

    public DriverSessionInfo? GetDriverInfo(int carIdx)
    {
        if (carIdx < 0 || carIdx >= DriverCount) return null;

        return new DriverSessionInfo
        {
            CarIdx = carIdx,
            UserName = $"Driver {carIdx + 1}",
            CarNumber = $"{carIdx + 1}",
            CarClassShortName = carIdx % 3 == 0 ? "GT3" : "GTE",
            CarScreenNameShort = "Demo Car",
            IsSpectator = false,
            IRating = 2000 + carIdx * 100,
            LicString = "A 4.50"
        };
    }
}
