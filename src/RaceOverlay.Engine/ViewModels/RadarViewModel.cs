using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RaceOverlay.Engine.Models;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.Engine.ViewModels;

public partial class RadarViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<RadarCar> cars = new();

    [ObservableProperty]
    private double rangeMeters = 40.0;

    [ObservableProperty]
    private string playerColor = "#3B82F6";

    [ObservableProperty]
    private string opponentColor = "#EF4444";

    public void ApplyConfiguration(IRadarConfig config)
    {
        RangeMeters = config.RangeMeters;
        PlayerColor = config.PlayerColor;
        OpponentColor = config.OpponentColor;
    }

    public void RefreshCars(IReadOnlyList<RadarCar> updatedCars)
    {
        Cars.Clear();
        foreach (var car in updatedCars)
        {
            Cars.Add(car);
        }
    }
}
