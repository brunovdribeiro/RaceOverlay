namespace RaceOverlay.Engine.Models;

/// <summary>
/// Data snapshot for the Lap Timer widget.
/// Updated atomically each tick — not an ObservableObject.
/// </summary>
public class LapTimerData
{
    public double CurrentLapTime { get; set; }
    public double LastLapTime { get; set; }
    public double BestLapTime { get; set; }
    public double DeltaToBest { get; set; }
    public double DeltaLastBest { get; set; }
    public int CurrentLap { get; set; }
    public int TotalLaps { get; set; }
    public bool IsOutLap { get; set; }
}
