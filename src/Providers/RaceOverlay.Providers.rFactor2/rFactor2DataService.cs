using RaceOverlay.Core.Services;
using rF2SharedMemoryNet;
using rF2SharedMemoryNet.RF2Data.Enums;
using rF2SharedMemoryNet.RF2Data.Structs;
using System.Diagnostics;
using System.Text;

namespace RaceOverlay.Providers.rFactor2;

public class rFactor2DataService : ILiveTelemetryService, IDisposable
{
    private readonly RF2MemoryReader _memoryReader;
    private volatile bool _isConnected;
    private CancellationTokenSource? _cts;
    private Task? _updateTask;
    private Telemetry? _latestTelemetry;
    private Scoring? _latestScoring;
    private readonly object _dataLock = new();

    public bool IsConnected => _isConnected;

    public event Action? TelemetryUpdated;
    public event Action? SessionInfoUpdated;
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public rFactor2DataService()
    {
        _memoryReader = new RF2MemoryReader();
    }

    public void Start()
    {
        if (_updateTask != null) return;

        _cts = new CancellationTokenSource();
        _updateTask = Task.Run(async () => await UpdateLoopAsync(_cts.Token), _cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _updateTask?.Wait(TimeSpan.FromSeconds(2));
        _updateTask = null;
        _cts?.Dispose();
        _cts = null;

        if (_isConnected)
        {
            _isConnected = false;
            OnDisconnected?.Invoke();
        }
    }

    private async Task UpdateLoopAsync(CancellationToken cancellationToken)
    {
        var wasConnected = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var isGameRunning = IsGameProcessRunning();

                if (isGameRunning)
                {
                    // Try to read telemetry
                    var telemetry = await _memoryReader.GetTelemetryAsync();
                    var scoring = await _memoryReader.GetScoringAsync();

                    if (telemetry != null && scoring != null)
                    {
                        lock (_dataLock)
                        {
                            _latestTelemetry = telemetry;
                            _latestScoring = scoring;
                        }

                        if (!wasConnected)
                        {
                            _isConnected = true;
                            wasConnected = true;
                            OnConnected?.Invoke();
                            SessionInfoUpdated?.Invoke();
                        }

                        TelemetryUpdated?.Invoke();
                    }
                }
                else
                {
                    if (wasConnected)
                    {
                        _isConnected = false;
                        wasConnected = false;
                        OnDisconnected?.Invoke();
                    }
                }

                // Update rate: ~50Hz for telemetry
                await Task.Delay(20, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Silently handle read errors
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    private static bool IsGameProcessRunning()
    {
        // Check for both rFactor 2 and Le Mans Ultimate
        var processes = Process.GetProcessesByName("rFactor2");
        if (processes.Length > 0) return true;

        processes = Process.GetProcessesByName("LMU");
        return processes.Length > 0;
    }

    // Telemetry access methods
    public float GetFloat(string variableName)
    {
        lock (_dataLock)
        {
            var playerTel = GetPlayerVehicleTelemetry();
            if (playerTel == null) return 0f;

            return variableName switch
            {
                "Speed" => CalculateSpeed(playerTel.Value.LocalVelocity),
                "RPM" => (float)playerTel.Value.EngineRPM,
                "Throttle" => (float)playerTel.Value.UnfilteredThrottle,
                "Brake" => (float)playerTel.Value.UnfilteredBrake,
                "Clutch" => (float)playerTel.Value.UnfilteredClutch,
                "Steering" => (float)playerTel.Value.UnfilteredSteering,
                "Fuel" => (float)playerTel.Value.Fuel,
                "FuelCapacity" => (float)playerTel.Value.FuelCapacity,
                _ => 0f
            };
        }
    }

    public int GetInt(string variableName)
    {
        lock (_dataLock)
        {
            if (_latestTelemetry == null || _latestScoring == null) return 0;

            var playerTel = GetPlayerVehicleTelemetry();
            var playerScore = GetPlayerVehicleScoring();

            return variableName switch
            {
                "Gear" => playerTel?.Gear ?? 0,
                "Lap" => playerScore?.TotalLaps ?? 0,
                "Position" => playerScore?.Place ?? 0,
                _ => 0
            };
        }
    }

    public bool GetBool(string variableName)
    {
        lock (_dataLock)
        {
            var playerScore = GetPlayerVehicleScoring();
            if (playerScore == null) return false;

            return variableName switch
            {
                "InPits" => playerScore.Value.InPits == 1,
                _ => false
            };
        }
    }

    // Array telemetry (for other cars)
    public float GetFloat(string variableName, int carIdx)
    {
        lock (_dataLock)
        {
            if (_latestTelemetry == null || carIdx < 0 || carIdx >= _latestTelemetry.Value.Vehicles.Length)
                return 0f;

            var vehicle = _latestTelemetry.Value.Vehicles[carIdx];

            return variableName switch
            {
                "Speed" => CalculateSpeed(vehicle.LocalVelocity),
                "RPM" => (float)vehicle.EngineRPM,
                _ => 0f
            };
        }
    }

    public int GetInt(string variableName, int carIdx)
    {
        lock (_dataLock)
        {
            if (_latestScoring == null || carIdx < 0 || carIdx >= _latestScoring.Value.Vehicles.Length)
                return 0;

            var vehicle = _latestScoring.Value.Vehicles[carIdx];

            return variableName switch
            {
                "Position" => vehicle.Place,
                "Lap" => vehicle.TotalLaps,
                _ => 0
            };
        }
    }

    public bool GetBool(string variableName, int carIdx)
    {
        lock (_dataLock)
        {
            if (_latestScoring == null || carIdx < 0 || carIdx >= _latestScoring.Value.Vehicles.Length)
                return false;

            var vehicle = _latestScoring.Value.Vehicles[carIdx];

            return variableName switch
            {
                "InPits" => vehicle.InPits == 1,
                _ => false
            };
        }
    }

    public float GetLapTime(string variableName)
    {
        lock (_dataLock)
        {
            var playerVehicle = GetPlayerVehicleScoring();
            if (playerVehicle == null) return 0f;

            return variableName switch
            {
                "CurrentLapTime" => (float)playerVehicle.Value.TimeIntoLap,
                "LastLapTime" => (float)playerVehicle.Value.LastLapTime,
                "BestLapTime" => (float)playerVehicle.Value.BestLapTime,
                _ => 0f
            };
        }
    }

    // ILiveTelemetryService properties
    public int DriverCount
    {
        get
        {
            lock (_dataLock)
            {
                return _latestScoring?.ScoringInfo.NumVehicles ?? 0;
            }
        }
    }

    public int PlayerCarIdx
    {
        get
        {
            lock (_dataLock)
            {
                if (_latestScoring == null) return -1;

                for (int i = 0; i < _latestScoring.Value.Vehicles.Length; i++)
                {
                    if (_latestScoring.Value.Vehicles[i].IsPlayer == 1)
                        return i;
                }

                return -1;
            }
        }
    }

    public DriverSessionInfo? GetDriverInfo(int carIdx)
    {
        lock (_dataLock)
        {
            if (_latestScoring == null || carIdx < 0 || carIdx >= _latestScoring.Value.Vehicles.Length)
                return null;

            var vehicle = _latestScoring.Value.Vehicles[carIdx];

            return new DriverSessionInfo
            {
                CarIdx = carIdx,
                UserName = ByteArrayToString(vehicle.DriverName),
                CarNumber = vehicle.ID.ToString(),
                CarClassShortName = ByteArrayToString(vehicle.VehicleClass),
                CarScreenNameShort = ByteArrayToString(vehicle.VehicleName),
                IsSpectator = false
            };
        }
    }

    public string? TrackName
    {
        get
        {
            lock (_dataLock)
            {
                if (_latestScoring == null) return null;
                return ByteArrayToString(_latestScoring.Value.ScoringInfo.TrackName);
            }
        }
    }

    public int TrackId => 0; // rF2 doesn't have track IDs

    public string? TrackConfigName => null; // Not available in rF2 shared memory

    public float TrackLengthKm
    {
        get
        {
            lock (_dataLock)
            {
                // Track length not available in rF2 shared memory - estimate from lap distance
                if (_latestScoring == null) return 0f;
                var player = GetPlayerVehicleScoring();
                if (player == null) return 0f;

                // LapDist gives normalized distance (0-1), not useful for absolute length
                return 0f; // Not available
            }
        }
    }

    public int SessionLaps
    {
        get
        {
            lock (_dataLock)
            {
                return _latestScoring?.ScoringInfo.MaxLaps ?? 0;
            }
        }
    }

    private VehicleScoring? GetPlayerVehicleScoring()
    {
        if (_latestScoring == null) return null;

        foreach (var vehicle in _latestScoring.Value.Vehicles)
        {
            if (vehicle.IsPlayer == 1)
                return vehicle;
        }

        return null;
    }

    private VehicleTelemetry? GetPlayerVehicleTelemetry()
    {
        if (_latestTelemetry == null || _latestTelemetry.Value.Vehicles.Length == 0) return null;

        // The first vehicle in telemetry is always the player
        return _latestTelemetry.Value.Vehicles[0];
    }

    private static float CalculateSpeed(Vec3 localVelocity)
    {
        // Calculate speed from velocity vector (magnitude)
        return (float)Math.Sqrt(
            localVelocity.X * localVelocity.X +
            localVelocity.Y * localVelocity.Y +
            localVelocity.Z * localVelocity.Z
        );
    }

    private static string ByteArrayToString(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return string.Empty;

        // Find null terminator
        int length = Array.IndexOf(bytes, (byte)0);
        if (length < 0) length = bytes.Length;

        return Encoding.ASCII.GetString(bytes, 0, length).Trim();
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
