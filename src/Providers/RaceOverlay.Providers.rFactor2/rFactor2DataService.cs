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

    // Cached track length (meters) derived from ScoringInfo.LapDist
    private double _trackLengthMeters;

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
        var processCheckInterval = 0; // Counter to throttle process checks
        var isGameRunning = true;     // Optimistic: assume running (we were told to start)

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Only check process every ~2 seconds (100 ticks × 20ms) instead of every tick
                if (processCheckInterval <= 0)
                {
                    isGameRunning = IsGameProcessRunning();
                    processCheckInterval = 100;
                }
                processCheckInterval--;

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

                            // Cache track length from ScoringInfo.LapDist (full lap distance in meters)
                            double lapDist = scoring.Value.ScoringInfo.LapDist;
                            if (lapDist > 0)
                                _trackLengthMeters = lapDist;
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

    internal static bool IsGameProcessRunning()
    {
        // Check for both rFactor 2 and Le Mans Ultimate
        var processes = Process.GetProcessesByName("rFactor2");
        if (processes.Length > 0) return true;

        processes = Process.GetProcessesByName("Le Mans Ultimate");
        return processes.Length > 0;
    }

    // Scalar telemetry (player car)
    public float GetFloat(string variableName)
    {
        lock (_dataLock)
        {
            var playerTel = GetPlayerVehicleTelemetry();
            var playerScore = GetPlayerVehicleScoring();

            return variableName switch
            {
                "Speed" => playerTel != null ? CalculateSpeed(playerTel.Value.LocalVelocity) : 0f,
                "RPM" => (float)(playerTel?.EngineRPM ?? 0),
                "Throttle" => (float)(playerTel?.UnfilteredThrottle ?? 0),
                "Brake" => (float)(playerTel?.UnfilteredBrake ?? 0),
                "Clutch" => (float)(playerTel?.UnfilteredClutch ?? 0),
                "Steering" => (float)(playerTel?.UnfilteredSteering ?? 0),
                "Fuel" => (float)(playerTel?.Fuel ?? 0),
                "FuelCapacity" => (float)(playerTel?.FuelCapacity ?? 0),

                // Fuel variables used by FuelCalculator widget
                "FuelLevel" => (float)(playerTel?.Fuel ?? 0),
                "FuelLevelPct" => playerTel != null && playerTel.Value.FuelCapacity > 0
                    ? (float)(playerTel.Value.Fuel / playerTel.Value.FuelCapacity)
                    : 0f,

                // Steering angle in radians (used by Inputs widget)
                "SteeringWheelAngle" => playerTel != null
                    ? (float)(playerTel.Value.UnfilteredSteering * playerTel.Value.PhysicalSteeringWheelRange * Math.PI / 360.0)
                    : 0f,

                // Lap timing (used by LapTimer widget)
                "LapCurrentLapTime" => (float)(playerScore?.TimeIntoLap ?? 0),
                "LapLastLapTime" => (float)(playerScore?.LastLapTime ?? 0),
                "LapBestLapTime" => (float)(playerScore?.BestLapTime ?? 0),
                "LapDeltaToBestLap" => CalculateDeltaToBest(playerScore),

                // Weather (used by Weather widget)
                "TrackTempCrew" => (float)(_latestScoring?.ScoringInfo.TrackTemp ?? 0),
                "AirTemp" => (float)(_latestScoring?.ScoringInfo.AmbientTemp ?? 0),
                "WindVel" => _latestScoring != null ? CalculateWindSpeed(_latestScoring.Value.ScoringInfo.Wind) : 0f,
                "WindDir" => _latestScoring != null ? CalculateWindDirection(_latestScoring.Value.ScoringInfo.Wind) : 0f,
                "RelativeHumidity" => 0f, // Not available in rF2 shared memory

                // Track map: Yaw relative to north (from orientation matrix)
                "YawNorth" => playerTel != null ? CalculateYawNorth(playerTel.Value.Orientation) : 0f,

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

                // Weather enums (used by Weather widget)
                "Skies" => EstimateSkies(),
                "Precipitation" => EstimatePrecipitation(),

                // CarLeftRight not available in rF2 — return 0 (none)
                "CarLeftRight" => 0,

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
                "OnPitRoad" => playerScore.Value.InPits == 1,
                _ => false
            };
        }
    }

    // Array telemetry (for other cars)
    public float GetFloat(string variableName, int carIdx)
    {
        lock (_dataLock)
        {
            // Some variables come from scoring, some from telemetry
            if (variableName is "CarIdxLapDistPct" or "CarIdxEstTime" or "CarIdxF2Time"
                or "CarIdxBestLapTime" or "CarIdxLastLapTime")
            {
                if (_latestScoring == null || carIdx < 0 || carIdx >= _latestScoring.Value.Vehicles.Length)
                    return 0f;

                var vehicle = _latestScoring.Value.Vehicles[carIdx];

                return variableName switch
                {
                    // Lap distance as percentage (0.0-1.0)
                    "CarIdxLapDistPct" => _trackLengthMeters > 0
                        ? (float)(vehicle.LapDist / _trackLengthMeters)
                        : 0f,

                    // Estimated time: use TimeIntoLap as approximation for estimated lap time
                    "CarIdxEstTime" => (float)vehicle.EstimatedLapTime,

                    // Time behind leader (used for gap calculations in Standings)
                    "CarIdxF2Time" => (float)vehicle.TimeBehindLeader,

                    "CarIdxBestLapTime" => (float)vehicle.BestLapTime,
                    "CarIdxLastLapTime" => (float)vehicle.LastLapTime,

                    _ => 0f
                };
            }

            // Telemetry-based array variables
            if (_latestTelemetry == null || carIdx < 0 || carIdx >= _latestTelemetry.Value.Vehicles.Length)
                return 0f;

            var telVehicle = _latestTelemetry.Value.Vehicles[carIdx];

            return variableName switch
            {
                "Speed" => CalculateSpeed(telVehicle.LocalVelocity),
                "RPM" => (float)telVehicle.EngineRPM,
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
                "CarIdxPosition" => vehicle.Place,
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
                "CarIdxOnPitRoad" => vehicle.InPits == 1,
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

    public int TrackId => 0; // rF2 doesn't have numeric track IDs

    public string? TrackConfigName => null; // Not available in rF2 shared memory

    public float TrackLengthKm
    {
        get
        {
            lock (_dataLock)
            {
                // ScoringInfo.LapDist is the full lap distance in meters
                return _trackLengthMeters > 0 ? (float)(_trackLengthMeters / 1000.0) : 0f;
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

    // --- Helper methods ---

    private VehicleScoring? GetPlayerVehicleScoring()
    {
        if (_latestScoring == null) return null;

        foreach (var vehicle in _latestScoring.Value.Vehicles)
        {
            if (vehicle.IsPlayer == 1 || (ControlEntity)vehicle.Control == ControlEntity.Player)
                return vehicle;
        }

        return null;
    }

    private VehicleTelemetry? GetPlayerVehicleTelemetry()
    {
        if (_latestTelemetry == null || _latestTelemetry.Value.Vehicles.Length == 0) return null;

        // Find the player vehicle in scoring first, then match by ID in telemetry
        var playerScoring = GetPlayerVehicleScoring();
        if (playerScoring == null)
            return _latestTelemetry.Value.Vehicles[0]; // fallback

        int playerId = playerScoring.Value.ID;
        foreach (var vehicle in _latestTelemetry.Value.Vehicles)
        {
            if (vehicle.ID == playerId)
                return vehicle;
        }

        return _latestTelemetry.Value.Vehicles[0]; // fallback
    }

    private static float CalculateSpeed(Vec3 localVelocity)
    {
        return (float)Math.Sqrt(
            localVelocity.X * localVelocity.X +
            localVelocity.Y * localVelocity.Y +
            localVelocity.Z * localVelocity.Z
        );
    }

    private static float CalculateWindSpeed(Vec3 wind)
    {
        return (float)Math.Sqrt(wind.X * wind.X + wind.Z * wind.Z);
    }

    private static float CalculateWindDirection(Vec3 wind)
    {
        // Wind direction in radians (from north, clockwise)
        return (float)Math.Atan2(wind.X, wind.Z);
    }

    private static float CalculateYawNorth(Vec3[] orientation)
    {
        if (orientation == null || orientation.Length < 3)
            return 0f;

        // Forward vector is the third row of the orientation matrix
        var forward = orientation[2];
        // Yaw relative to north: atan2(forward.X, forward.Z)
        return (float)Math.Atan2(forward.X, forward.Z);
    }

    private float CalculateDeltaToBest(VehicleScoring? playerScore)
    {
        if (playerScore == null) return 0f;

        double bestLap = playerScore.Value.BestLapTime;
        double timeIntoLap = playerScore.Value.TimeIntoLap;
        double estimatedLap = playerScore.Value.EstimatedLapTime;

        // If no best lap yet, delta is 0
        if (bestLap <= 0 || estimatedLap <= 0) return 0f;

        // Delta = estimated current lap time - best lap time
        return (float)(estimatedLap - bestLap);
    }

    private int EstimateSkies()
    {
        // Estimate sky condition from DarkCloud (0.0-1.0) and Raining
        if (_latestScoring == null) return 0;

        double darkCloud = _latestScoring.Value.ScoringInfo.DarkCloud;
        double raining = _latestScoring.Value.ScoringInfo.Raining;

        if (raining > 0.1) return 3; // Overcast/stormy
        if (darkCloud > 0.6) return 2; // Mostly cloudy
        if (darkCloud > 0.3) return 1; // Partly cloudy
        return 0; // Clear
    }

    private int EstimatePrecipitation()
    {
        if (_latestScoring == null) return 0;

        double raining = _latestScoring.Value.ScoringInfo.Raining;

        if (raining > 0.5) return 2; // Heavy rain
        if (raining > 0.1) return 1; // Light rain
        return 0; // No precipitation
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
