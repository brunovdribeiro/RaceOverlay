using RaceOverlay.Core.Services;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

public class FuelCalculatorConfig : IFuelCalculatorConfig
{
    public double FuelTankCapacity { get; set; } = 110.0;
    public int UpdateIntervalMs { get; set; } = 1000;
    public bool UseMockData { get; set; } = false;
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;
}

public class FuelCalculator : IWidget
{
    private FuelCalculatorConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;
    private readonly ILiveTelemetryService? _telemetryService;

    // Mock state
    private double _fuelRemaining;
    private double _fuelPerLap = 3.0;
    private int _currentLap = 1;
    private int _totalLaps = 30;
    private double _lapProgress;
    private readonly Random _random = new();

    // Live fuel tracking
    private float _lastLapFuelLevel;
    private int _lastLap;
    private double _liveFuelPerLap;
    private int _completedLaps;
    private double _totalFuelUsed;

    public event Action? DataUpdated;

    public string WidgetId => "fuel-calculator";
    public string DisplayName => "Fuel Calculator";
    public string Description => "Tracks fuel remaining, consumption rate, and calculates fuel needed for pit stops.";
    public IWidgetConfiguration Configuration => _configuration;

    private bool UseLiveData => !_configuration.UseMockData
                                && _telemetryService?.IsConnected == true;

    public FuelCalculator(ILiveTelemetryService? telemetryService = null)
    {
        _configuration = new FuelCalculatorConfig();
        _telemetryService = telemetryService;
    }

    public void UpdateConfiguration(IWidgetConfiguration configuration)
    {
        if (configuration is FuelCalculatorConfig config)
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
        else
        {
            _lastLap = -1;
            _liveFuelPerLap = 0;
            _completedLaps = 0;
            _totalFuelUsed = 0;
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
    }

    private void InitializeMockData()
    {
        _fuelRemaining = 80.0;
        _fuelPerLap = 3.0 + _random.NextDouble() * 0.4 - 0.2;
        _currentLap = 1;
        _totalLaps = 30;
        _lapProgress = 0;
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!UseLiveData)
                {
                    UpdateMockFuel();
                }
                else
                {
                    UpdateLiveFuelTracking();
                }

                DataUpdated?.Invoke();

                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void UpdateMockFuel()
    {
        double consumption = 0.05 + (_random.NextDouble() - 0.5) * 0.01;
        _fuelRemaining = Math.Max(0, _fuelRemaining - consumption);

        _lapProgress += 1.0 / 60.0 + (_random.NextDouble() - 0.5) * 0.005;
        if (_lapProgress >= 1.0)
        {
            _lapProgress = 0;
            if (_currentLap < _totalLaps)
            {
                _currentLap++;
                _fuelPerLap = 3.0 + (_random.NextDouble() - 0.5) * 0.4;
            }
        }
    }

    private void UpdateLiveFuelTracking()
    {
        var ts = _telemetryService!;
        int currentLap = ts.GetInt("Lap");
        float fuelLevel = ts.GetFloat("FuelLevel");

        // Detect lap change to compute fuel per lap
        if (_lastLap >= 0 && currentLap > _lastLap && _lastLapFuelLevel > 0)
        {
            double fuelUsedThisLap = _lastLapFuelLevel - fuelLevel;
            if (fuelUsedThisLap > 0)
            {
                _totalFuelUsed += fuelUsedThisLap;
                _completedLaps++;
                _liveFuelPerLap = _totalFuelUsed / _completedLaps;
            }
        }

        _lastLap = currentLap;
        _lastLapFuelLevel = fuelLevel;
    }

    public FuelData GetFuelData()
    {
        if (UseLiveData)
        {
            return GetLiveFuelData();
        }

        return GetMockFuelData();
    }

    private FuelData GetLiveFuelData()
    {
        var ts = _telemetryService!;

        float fuelLevel = ts.GetFloat("FuelLevel");
        float fuelLevelPct = ts.GetFloat("FuelLevelPct");
        int currentLap = ts.GetInt("Lap");
        int totalLaps = ts.SessionLaps;

        double fuelPerLap = _liveFuelPerLap > 0 ? _liveFuelPerLap : 3.0; // default estimate
        double fuelRemainingLaps = fuelPerLap > 0 ? fuelLevel / fuelPerLap : 0;

        int lapsRemaining = totalLaps > 0 ? totalLaps - currentLap : 0;
        double fuelToFinish = lapsRemaining * fuelPerLap;
        double fuelToAdd = Math.Max(0, fuelToFinish - fuelLevel);

        // Get tank capacity from session info
        double tankCapacity = _configuration.FuelTankCapacity;
        if (fuelLevelPct > 0 && fuelLevel > 0)
        {
            tankCapacity = fuelLevel / fuelLevelPct;
        }

        return new FuelData
        {
            FuelRemaining = fuelLevel,
            FuelTankCapacity = tankCapacity,
            FuelPerLap = fuelPerLap,
            FuelRemainingLaps = fuelRemainingLaps,
            CurrentLap = currentLap,
            TotalLaps = totalLaps,
            LapsRemaining = lapsRemaining,
            FuelToFinish = fuelToFinish,
            FuelToAdd = fuelToAdd,
            FuelRemainingPercent = fuelLevelPct * 100
        };
    }

    private FuelData GetMockFuelData()
    {
        int lapsRemaining = _totalLaps - _currentLap;
        double fuelRemainingLaps = _fuelPerLap > 0 ? _fuelRemaining / _fuelPerLap : 0;
        double fuelToFinish = lapsRemaining * _fuelPerLap;
        double fuelToAdd = Math.Max(0, fuelToFinish - _fuelRemaining);
        double fuelPercent = _configuration.FuelTankCapacity > 0
            ? _fuelRemaining / _configuration.FuelTankCapacity * 100
            : 0;

        return new FuelData
        {
            FuelRemaining = _fuelRemaining,
            FuelTankCapacity = _configuration.FuelTankCapacity,
            FuelPerLap = _fuelPerLap,
            FuelRemainingLaps = fuelRemainingLaps,
            CurrentLap = _currentLap,
            TotalLaps = _totalLaps,
            LapsRemaining = lapsRemaining,
            FuelToFinish = fuelToFinish,
            FuelToAdd = fuelToAdd,
            FuelRemainingPercent = fuelPercent
        };
    }
}
