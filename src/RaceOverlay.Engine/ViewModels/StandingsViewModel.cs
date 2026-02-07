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
    private bool showBestLapTime = true;

    [ObservableProperty]
    private bool showGap = true;

    [ObservableProperty]
    private int currentLap;

    [ObservableProperty]
    private int totalLaps;

    public void ApplyConfiguration(IStandingsConfig config)
    {
        ShowClassColor = config.ShowClassColor;
        ShowBestLapTime = config.ShowBestLapTime;
        ShowGap = config.ShowGap;
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
