using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
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

    public RelativeOverlayViewModel()
    {
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

    /// <summary>
    /// Formats a lap time (in seconds) to MM:SS.SSS format.
    /// </summary>
    public static string FormatLapTime(double seconds)
    {
        var timespan = TimeSpan.FromSeconds(seconds);
        return timespan.ToString(@"mm\:ss\.fff");
    }

    /// <summary>
    /// Formats a gap to next driver (in meters) with direction indicator.
    /// </summary>
    public static string FormatGap(double gapMeters)
    {
        if (gapMeters < 0)
            return $"{Math.Abs(gapMeters):F1}m ▼";
        else
            return $"{gapMeters:F1}m ▲";
    }

    /// <summary>
    /// Formats delta from best lap with arrow indicator.
    /// </summary>
    public static string FormatDelta(double deltaSeconds)
    {
        if (Math.Abs(deltaSeconds) < 0.01)
            return "0.000s";
        else if (deltaSeconds < 0)
            return $"{Math.Abs(deltaSeconds):F3}s ▲";
        else
            return $"+{deltaSeconds:F3}s ▼";
    }

    /// <summary>
    /// Gets a color based on delta time for visual feedback.
    /// Green for fast (within 0.5s), yellow for medium, red for slow.
    /// </summary>
    public static string GetDeltaColor(double deltaSeconds)
    {
        if (deltaSeconds <= 0.5)
            return "#22C55E"; // Green
        else if (deltaSeconds <= 1.5)
            return "#FBBF24"; // Yellow
        else
            return "#EF4444"; // Red
    }

    /// <summary>
    /// Gets a stint progress bar color based on laps remaining.
    /// </summary>
    public static string GetStintColor(int completed, int total)
    {
        double percentage = (double)completed / total;

        if (percentage >= 0.8)
            return "#EF4444"; // Red - near end of stint
        else if (percentage >= 0.5)
            return "#FBBF24"; // Yellow - midway
        else
            return "#22C55E"; // Green - plenty of laps left
    }

    /// <summary>
    /// Gets formatted stint display (e.g., "12/35").
    /// </summary>
    public static string FormatStint(int completed, int total)
    {
        return $"{completed}/{total}";
    }
}
