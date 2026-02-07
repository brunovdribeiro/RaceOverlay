using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Widgets;

public class FuelCalculatorConfig : IFuelCalculatorConfig
{
    public double FuelTankCapacity { get; set; } = 110.0;
    public int UpdateIntervalMs { get; set; } = 1000;
    public bool UseMockData { get; set; } = true;
    public double OverlayLeft { get; set; } = double.NaN;
    public double OverlayTop { get; set; } = double.NaN;
}

public class FuelCalculator : IWidget
{
    private FuelCalculatorConfig _configuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _updateTask;

    // Mock state
    private double _fuelRemaining;
    private double _fuelPerLap = 3.0;
    private int _currentLap = 1;
    private int _totalLaps = 30;
    private double _lapProgress; // 0..1 within current lap
    private readonly Random _random = new();

    public event Action? DataUpdated;

    public string WidgetId => "fuel-calculator";
    public string DisplayName => "Fuel Calculator";
    public string Description => "Tracks fuel remaining, consumption rate, and calculates fuel needed for pit stops.";
    public IWidgetConfiguration Configuration => _configuration;

    public FuelCalculator()
    {
        _configuration = new FuelCalculatorConfig();
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
    }

    private void InitializeMockData()
    {
        _fuelRemaining = 80.0;
        _fuelPerLap = 3.0 + _random.NextDouble() * 0.4 - 0.2; // ~2.8-3.2
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
                // Each tick simulates ~0.05L consumption (at 1000ms interval, ~60 ticks/lap = ~3L/lap)
                double consumption = 0.05 + (_random.NextDouble() - 0.5) * 0.01;
                _fuelRemaining = Math.Max(0, _fuelRemaining - consumption);

                // Advance lap progress (~1/60 per tick for ~60s laps)
                _lapProgress += 1.0 / 60.0 + (_random.NextDouble() - 0.5) * 0.005;
                if (_lapProgress >= 1.0)
                {
                    _lapProgress = 0;
                    if (_currentLap < _totalLaps)
                    {
                        _currentLap++;
                        // Slight variation in fuel per lap each lap
                        _fuelPerLap = 3.0 + (_random.NextDouble() - 0.5) * 0.4;
                    }
                }

                DataUpdated?.Invoke();

                await Task.Delay(_configuration.UpdateIntervalMs, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public FuelData GetFuelData()
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
