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

    [ObservableProperty]
    private bool showBlindSpotIndicators = true;

    [ObservableProperty]
    private bool carOnLeft;

    [ObservableProperty]
    private bool carOnRight;

    [ObservableProperty]
    private string leftIndicatorColor = "#EF4444";

    [ObservableProperty]
    private string rightIndicatorColor = "#EF4444";

    public void ApplyConfiguration(IRadarConfig config)
    {
        RangeMeters = config.RangeMeters;
        PlayerColor = config.PlayerColor;
        OpponentColor = config.OpponentColor;
        ShowBlindSpotIndicators = config.ShowBlindSpotIndicators;
    }

    public void RefreshCars(IReadOnlyList<RadarCar> updatedCars)
    {
        Cars.Clear();
        foreach (var car in updatedCars)
        {
            Cars.Add(car);
        }

        UpdateBlindSpotIndicators();
    }

    private void UpdateBlindSpotIndicators()
    {
        const double lateralThreshold = 1.0;
        const double longitudinalThreshold = 8.0;

        bool left = false;
        bool right = false;
        double closestLeftDist = double.MaxValue;
        double closestRightDist = double.MaxValue;

        foreach (var car in Cars)
        {
            if (car.IsPlayer) continue;
            if (Math.Abs(car.LongitudinalOffset) > longitudinalThreshold) continue;

            double dist = Math.Sqrt(car.LateralOffset * car.LateralOffset + car.LongitudinalOffset * car.LongitudinalOffset);

            if (car.LateralOffset < -lateralThreshold)
            {
                left = true;
                if (dist < closestLeftDist)
                {
                    closestLeftDist = dist;
                    LeftIndicatorColor = car.Color;
                }
            }

            if (car.LateralOffset > lateralThreshold)
            {
                right = true;
                if (dist < closestRightDist)
                {
                    closestRightDist = dist;
                    RightIndicatorColor = car.Color;
                }
            }
        }

        CarOnLeft = left;
        CarOnRight = right;
    }
}
