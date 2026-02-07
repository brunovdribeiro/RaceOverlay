namespace RaceOverlay.Engine.Models;

public class FuelData
{
    public double FuelRemaining { get; set; }
    public double FuelTankCapacity { get; set; }
    public double FuelPerLap { get; set; }
    public double FuelRemainingLaps { get; set; }
    public int CurrentLap { get; set; }
    public int TotalLaps { get; set; }
    public int LapsRemaining { get; set; }
    public double FuelToFinish { get; set; }
    public double FuelToAdd { get; set; }
    public double FuelRemainingPercent { get; set; }
}
