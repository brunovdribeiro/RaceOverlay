using CommunityToolkit.Mvvm.ComponentModel;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.ViewModels;

public partial class TrackMapViewModel : ObservableObject
{
    [ObservableProperty]
    private int currentLap;

    [ObservableProperty]
    private int totalLaps;

    [ObservableProperty]
    private bool showDriverNames;

    [ObservableProperty]
    private bool showPitStatus = true;

    public IReadOnlyList<TrackMapDriver> Drivers { get; private set; } = Array.Empty<TrackMapDriver>();

    public (double X, double Y)[] TrackOutline { get; set; } = Array.Empty<(double, double)>();

    public event Action? MapUpdated;

    public void NotifyOutlineChanged()
    {
        MapUpdated?.Invoke();
    }

    public void ApplyConfiguration(ITrackMapConfig config)
    {
        ShowDriverNames = config.ShowDriverNames;
        ShowPitStatus = config.ShowPitStatus;
    }

    public void UpdateMap(IReadOnlyList<TrackMapDriver> drivers, int currentLap, int totalLaps)
    {
        Drivers = drivers;
        CurrentLap = currentLap;
        TotalLaps = totalLaps;
        MapUpdated?.Invoke();
    }
}
