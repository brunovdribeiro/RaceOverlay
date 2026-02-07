namespace RaceOverlay.Engine.Models;

public class TrackMapDriver
{
    public string DriverName { get; set; } = string.Empty;
    public string ClassColor { get; set; } = "#6B7280";
    public double TrackProgress { get; set; }
    public bool IsPlayer { get; set; }
    public bool IsInPit { get; set; }
}
