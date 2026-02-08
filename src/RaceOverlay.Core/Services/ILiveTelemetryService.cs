namespace RaceOverlay.Core.Services;

public interface ILiveTelemetryService
{
    bool IsConnected { get; }

    event Action? TelemetryUpdated;
    event Action? SessionInfoUpdated;
    event Action? OnConnected;
    event Action? OnDisconnected;

    // Scalar telemetry (player car)
    float GetFloat(string variableName);
    int GetInt(string variableName);
    bool GetBool(string variableName);

    // Array telemetry (CarIdx* variables)
    float GetFloat(string variableName, int carIdx);
    int GetInt(string variableName, int carIdx);
    bool GetBool(string variableName, int carIdx);

    // Session info
    int DriverCount { get; }
    int PlayerCarIdx { get; }
    DriverSessionInfo? GetDriverInfo(int carIdx);
    string? TrackName { get; }
    float TrackLengthKm { get; }
    int SessionLaps { get; }

    void Start();
    void Stop();
}
