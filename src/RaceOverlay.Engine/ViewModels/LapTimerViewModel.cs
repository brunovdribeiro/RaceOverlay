using CommunityToolkit.Mvvm.ComponentModel;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.ViewModels;

/// <summary>
/// ViewModel for the Lap Timer widget.
/// Manages lap timing display with MVVM binding.
/// </summary>
public partial class LapTimerViewModel : ObservableObject
{
    [ObservableProperty]
    private double currentLapTime;

    [ObservableProperty]
    private double lastLapTime;

    [ObservableProperty]
    private double bestLapTime;

    [ObservableProperty]
    private double deltaToBest;

    [ObservableProperty]
    private double deltaLastBest;

    [ObservableProperty]
    private int currentLap;

    [ObservableProperty]
    private int totalLaps;

    [ObservableProperty]
    private bool isOutLap;

    [ObservableProperty]
    private bool showDeltaToBest = true;

    [ObservableProperty]
    private bool showLastLap = true;

    [ObservableProperty]
    private bool showBestLap = true;

    [ObservableProperty]
    private bool showDeltaLastBest = true;

    public void ApplyConfiguration(ILapTimerConfig config)
    {
        ShowDeltaToBest = config.ShowDeltaToBest;
        ShowLastLap = config.ShowLastLap;
        ShowBestLap = config.ShowBestLap;
        ShowDeltaLastBest = config.ShowDeltaLastBest;
    }

    public void UpdateLapData(LapTimerData data)
    {
        CurrentLapTime = data.CurrentLapTime;
        LastLapTime = data.LastLapTime;
        BestLapTime = data.BestLapTime;
        DeltaToBest = data.DeltaToBest;
        DeltaLastBest = data.DeltaLastBest;
        CurrentLap = data.CurrentLap;
        TotalLaps = data.TotalLaps;
        IsOutLap = data.IsOutLap;
    }
}
