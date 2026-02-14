namespace RaceOverlay.Engine.Models;

public class RadarCar
{
    public int CarIdx { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public double LateralOffset { get; set; } // -1 to 1 or similar
    public double LongitudinalOffset { get; set; } // meters
    public bool IsPlayer { get; set; }
    public string Color { get; set; } = "#FFFFFF";
}
