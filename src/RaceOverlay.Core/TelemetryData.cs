namespace RaceOverlay.Core.Providers;

/// <summary>
/// Base telemetry data model
/// </summary>
public class TelemetryData
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public float Speed { get; init; }
    public float Rpm { get; init; }
    public int Gear { get; init; }
    public float Throttle { get; init; }
    public float Brake { get; init; }
    public float Clutch { get; init; }
    public TimeSpan? CurrentLapTime { get; init; }
    public TimeSpan? LastLapTime { get; init; }
    public TimeSpan? BestLapTime { get; init; }
    public int LapNumber { get; init; }
    public string? TrackName { get; init; }
    public string? CarName { get; init; }
}
