using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.ViewModels;

/// <summary>
/// ViewModel for the Relative Overlay widget.
/// Manages the display of relative driver data with MVVM binding.
/// </summary>
public partial class RelativeOverlayViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<RelativeDriver> relativeDrivers = new();

    [ObservableProperty]
    private RelativeDriver? playerDriver;

    [ObservableProperty]
    private ObservableCollection<RelativeDriver> driversAhead = new();

    [ObservableProperty]
    private ObservableCollection<RelativeDriver> driversBehind = new();

    [ObservableProperty]
    private bool showPosition = true;

    [ObservableProperty]
    private bool showClassColor = true;

    [ObservableProperty]
    private bool showDriverName = true;

    [ObservableProperty]
    private bool showRating = true;

    [ObservableProperty]
    private bool showStint = true;

    [ObservableProperty]
    private bool showLapTime = true;

    [ObservableProperty]
    private bool showGap = true;

    public RelativeOverlayViewModel()
    {
    }

    /// <summary>
    /// Copies column visibility toggle values from the widget configuration.
    /// </summary>
    public void ApplyConfiguration(IRelativeOverlayConfig config)
    {
        ShowPosition = config.ShowPosition;
        ShowClassColor = config.ShowClassColor;
        ShowDriverName = config.ShowDriverName;
        ShowRating = config.ShowRating;
        ShowStint = config.ShowStint;
        ShowLapTime = config.ShowLapTime;
        ShowGap = config.ShowGap;
    }

    /// <summary>
    /// Loads relative drivers and organizes them into sections.
    /// </summary>
    public void LoadRelativeDrivers(IReadOnlyList<RelativeDriver> drivers)
    {
        RelativeDrivers.Clear();
        DriversAhead.Clear();
        DriversBehind.Clear();
        PlayerDriver = null;

        foreach (var driver in drivers)
        {
            RelativeDrivers.Add(driver);

            switch (driver.RelativePosition)
            {
                case -1: // Ahead
                    DriversAhead.Add(driver);
                    break;
                case 0: // Player
                    PlayerDriver = driver;
                    break;
                case 1: // Behind
                    DriversBehind.Add(driver);
                    break;
            }
        }
    }

    /// <summary>
    /// Refreshes the relative drivers collection.
    /// Call this when driver data has been updated by the widget.
    /// </summary>
    public void RefreshDrivers(IReadOnlyList<RelativeDriver> drivers)
    {
        LoadRelativeDrivers(drivers);
    }
}
