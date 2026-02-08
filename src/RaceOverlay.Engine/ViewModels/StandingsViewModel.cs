using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.ViewModels;

/// <summary>
/// ViewModel for the Standings widget.
/// Manages the race leaderboard display with MVVM binding.
/// </summary>
public partial class StandingsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<StandingDriver> drivers = new();

    [ObservableProperty]
    private bool showClassColor = true;

    [ObservableProperty]
    private bool showCarNumber = true;

    [ObservableProperty]
    private bool showPositionsGained = true;

    [ObservableProperty]
    private bool showLicense = true;

    [ObservableProperty]
    private bool showIRating = true;

    [ObservableProperty]
    private bool showCarBrand = true;

    [ObservableProperty]
    private bool showInterval = true;

    [ObservableProperty]
    private bool showGap = true;

    [ObservableProperty]
    private bool showLastLapTime = true;

    [ObservableProperty]
    private bool showDelta = true;

    [ObservableProperty]
    private bool showPitStatus = true;

    [ObservableProperty]
    private int currentLap;

    [ObservableProperty]
    private int totalLaps;

    public void ApplyConfiguration(IStandingsConfig config)
    {
        ShowClassColor = config.ShowClassColor;
        ShowCarNumber = config.ShowCarNumber;
        ShowPositionsGained = config.ShowPositionsGained;
        ShowLicense = config.ShowLicense;
        ShowIRating = config.ShowIRating;
        ShowCarBrand = config.ShowCarBrand;
        ShowInterval = config.ShowInterval;
        ShowGap = config.ShowGap;
        ShowLastLapTime = config.ShowLastLapTime;
        ShowDelta = config.ShowDelta;
        ShowPitStatus = config.ShowPitStatus;
    }

    public void UpdateStandings(IReadOnlyList<StandingDriver> standings, int currentLap, int totalLaps)
    {
        Drivers.Clear();
        foreach (var driver in standings)
        {
            Drivers.Add(driver);
        }
        CurrentLap = currentLap;
        TotalLaps = totalLaps;
    }
}
