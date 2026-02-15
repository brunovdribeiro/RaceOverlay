namespace RaceOverlay.Core.Services;

/// <summary>
/// A proxy that dynamically delegates all calls to the currently active telemetry
/// service from the GameDetectionService. This ensures widgets always talk to the
/// correct service (demo or live) even when the active game changes at runtime.
/// 
/// All property/method calls are delegated at call time so they always reach the
/// current service. Events are NOT proxied — widgets use their own polling loops
/// to call GetFloat/GetInt, so they automatically pick up the live data as soon
/// as Current switches.
/// </summary>
public class TelemetryServiceProxy : ILiveTelemetryService
{
    private readonly Func<ILiveTelemetryService> _serviceResolver;

    private ILiveTelemetryService Current => _serviceResolver();

    public TelemetryServiceProxy(Func<ILiveTelemetryService> serviceResolver)
    {
        _serviceResolver = serviceResolver;
    }

    public bool IsConnected => Current.IsConnected;

    // Events: not heavily used by widgets (they poll), but we expose simple
    // backing delegates so nothing throws at subscribe time.
    public event Action? TelemetryUpdated;
    public event Action? SessionInfoUpdated;
    public event Action? OnConnected;
    public event Action? OnDisconnected;

    /// <summary>Raise TelemetryUpdated from external code if needed.</summary>
    internal void RaiseTelemetryUpdated() => TelemetryUpdated?.Invoke();

    // Scalar telemetry
    public float GetFloat(string variableName) => Current.GetFloat(variableName);
    public int GetInt(string variableName) => Current.GetInt(variableName);
    public bool GetBool(string variableName) => Current.GetBool(variableName);

    // Array telemetry
    public float GetFloat(string variableName, int carIdx) => Current.GetFloat(variableName, carIdx);
    public int GetInt(string variableName, int carIdx) => Current.GetInt(variableName, carIdx);
    public bool GetBool(string variableName, int carIdx) => Current.GetBool(variableName, carIdx);

    // Session info
    public int DriverCount => Current.DriverCount;
    public int PlayerCarIdx => Current.PlayerCarIdx;
    public DriverSessionInfo? GetDriverInfo(int carIdx) => Current.GetDriverInfo(carIdx);
    public string? TrackName => Current.TrackName;
    public int TrackId => Current.TrackId;
    public string? TrackConfigName => Current.TrackConfigName;
    public float TrackLengthKm => Current.TrackLengthKm;
    public int SessionLaps => Current.SessionLaps;

    public void Start() => Current.Start();
    public void Stop() => Current.Stop();
}
