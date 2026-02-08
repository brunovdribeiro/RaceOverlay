using IRSDKSharper;
using RaceOverlay.Core.Services;

namespace RaceOverlay.Providers.iRacing;

public class IRacingDataService : ILiveTelemetryService
{
    private readonly IRacingSdk _sdk;
    private volatile bool _isConnected;
    private List<DriverSessionInfo>? _drivers;
    private readonly object _driversLock = new();

    public bool IsConnected => _isConnected;

    public event Action? TelemetryUpdated;
    public event Action? SessionInfoUpdated;
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    public IRacingDataService()
    {
        _sdk = new IRacingSdk();

        _sdk.OnConnected += HandleConnected;
        _sdk.OnDisconnected += HandleDisconnected;
        _sdk.OnTelemetryData += HandleTelemetryData;
        _sdk.OnSessionInfo += HandleSessionInfo;
    }

    public void Start()
    {
        _sdk.Start();
    }

    public void Stop()
    {
        _sdk.Stop();
        _isConnected = false;
    }

    private void HandleConnected()
    {
        _isConnected = true;
        OnConnected?.Invoke();
    }

    private void HandleDisconnected()
    {
        _isConnected = false;
        lock (_driversLock)
        {
            _drivers = null;
        }
        OnDisconnected?.Invoke();
    }

    private void HandleTelemetryData()
    {
        TelemetryUpdated?.Invoke();
    }

    private void HandleSessionInfo()
    {
        UpdateDriverCache();
        SessionInfoUpdated?.Invoke();
    }

    // Scalar telemetry
    public float GetFloat(string variableName) => _sdk.Data.GetFloat(variableName);
    public int GetInt(string variableName) => _sdk.Data.GetInt(variableName);
    public bool GetBool(string variableName) => _sdk.Data.GetBool(variableName);

    // Array telemetry
    public float GetFloat(string variableName, int carIdx) => _sdk.Data.GetFloat(variableName, carIdx);
    public int GetInt(string variableName, int carIdx) => _sdk.Data.GetInt(variableName, carIdx);
    public bool GetBool(string variableName, int carIdx) => _sdk.Data.GetBool(variableName, carIdx);

    public int DriverCount
    {
        get
        {
            lock (_driversLock)
            {
                return _drivers?.Count ?? 0;
            }
        }
    }

    public int PlayerCarIdx
    {
        get
        {
            try
            {
                return _sdk.Data.SessionInfo?.DriverInfo?.DriverCarIdx ?? -1;
            }
            catch
            {
                return -1;
            }
        }
    }

    public DriverSessionInfo? GetDriverInfo(int carIdx)
    {
        lock (_driversLock)
        {
            if (_drivers == null) return null;
            return _drivers.FirstOrDefault(d => d.CarIdx == carIdx);
        }
    }

    public string? TrackName
    {
        get
        {
            try
            {
                return _sdk.Data.SessionInfo?.WeekendInfo?.TrackDisplayName
                    ?? _sdk.Data.SessionInfo?.WeekendInfo?.TrackName;
            }
            catch
            {
                return null;
            }
        }
    }

    public float TrackLengthKm
    {
        get
        {
            try
            {
                var trackLength = _sdk.Data.SessionInfo?.WeekendInfo?.TrackLength;
                if (trackLength != null && trackLength.Contains("km"))
                {
                    var numStr = trackLength.Replace("km", "").Trim();
                    if (float.TryParse(numStr, System.Globalization.CultureInfo.InvariantCulture, out var km))
                        return km;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }

    public int SessionLaps
    {
        get
        {
            try
            {
                var sessions = _sdk.Data.SessionInfo?.SessionInfo?.Sessions;
                if (sessions == null) return 0;

                var currentSessionNum = _sdk.Data.GetInt("SessionNum");
                if (currentSessionNum >= 0 && currentSessionNum < sessions.Count)
                {
                    var lapsStr = sessions[currentSessionNum].SessionLaps;
                    if (lapsStr != null && lapsStr != "unlimited" && int.TryParse(lapsStr, out var laps))
                        return laps;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }

    private void UpdateDriverCache()
    {
        try
        {
            var sdkDrivers = _sdk.Data.SessionInfo?.DriverInfo?.Drivers;
            if (sdkDrivers == null) return;

            var newDrivers = new List<DriverSessionInfo>();

            foreach (var d in sdkDrivers)
            {
                newDrivers.Add(new DriverSessionInfo
                {
                    CarIdx = d.CarIdx,
                    UserName = d.UserName ?? string.Empty,
                    CarNumber = d.CarNumber ?? string.Empty,
                    CarClassShortName = d.CarClassShortName ?? string.Empty,
                    CarClassColor = ParseHexColor(d.CarClassColor),
                    IRating = d.IRating,
                    LicString = d.LicString ?? string.Empty,
                    LicColor = ParseHexColor(d.LicColor),
                    CarScreenNameShort = d.CarScreenNameShort ?? string.Empty,
                    IsSpectator = d.IsSpectator != 0
                });
            }

            lock (_driversLock)
            {
                _drivers = newDrivers;
            }
        }
        catch
        {
            // Silently handle parsing errors during session info update
        }
    }

    private static int ParseHexColor(object? value)
    {
        if (value == null) return 0;

        if (value is int intVal) return intVal;
        if (value is long longVal) return (int)longVal;

        var str = value.ToString();
        if (str == null) return 0;

        str = str.TrimStart('#').Replace("0x", "");
        if (int.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out var result))
            return result;

        if (int.TryParse(str, out var decResult))
            return decResult;

        return 0;
    }
}
