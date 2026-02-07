using CommunityToolkit.Mvvm.ComponentModel;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.ViewModels;

public partial class FuelCalculatorViewModel : ObservableObject
{
    [ObservableProperty]
    private double fuelRemaining;

    [ObservableProperty]
    private double fuelTankCapacity = 110.0;

    [ObservableProperty]
    private double fuelPerLap;

    [ObservableProperty]
    private double fuelRemainingLaps;

    [ObservableProperty]
    private int currentLap;

    [ObservableProperty]
    private int totalLaps;

    [ObservableProperty]
    private int lapsRemaining;

    [ObservableProperty]
    private double fuelToFinish;

    [ObservableProperty]
    private double fuelToAdd;

    [ObservableProperty]
    private double fuelRemainingPercent;

    public void ApplyConfiguration(IFuelCalculatorConfig config)
    {
        FuelTankCapacity = config.FuelTankCapacity;
    }

    public void UpdateFuelData(FuelData data)
    {
        FuelRemaining = data.FuelRemaining;
        FuelTankCapacity = data.FuelTankCapacity;
        FuelPerLap = data.FuelPerLap;
        FuelRemainingLaps = data.FuelRemainingLaps;
        CurrentLap = data.CurrentLap;
        TotalLaps = data.TotalLaps;
        LapsRemaining = data.LapsRemaining;
        FuelToFinish = data.FuelToFinish;
        FuelToAdd = data.FuelToAdd;
        FuelRemainingPercent = data.FuelRemainingPercent;
    }
}
