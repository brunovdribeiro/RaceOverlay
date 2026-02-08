using CommunityToolkit.Mvvm.ComponentModel;

namespace RaceOverlay.Engine.Models;

/// <summary>
/// Represents a driver in the race standings leaderboard.
/// Uses ObservableObject so WPF detects property changes when the update loop mutates driver data.
/// </summary>
public partial class StandingDriver : ObservableObject
{
    [ObservableProperty]
    private int position;

    [ObservableProperty]
    private string driverName = string.Empty;

    [ObservableProperty]
    private string vehicleClass = string.Empty;

    [ObservableProperty]
    private string classColor = "#6B7280";

    [ObservableProperty]
    private string carNumber = string.Empty;

    [ObservableProperty]
    private int startingPosition;

    [ObservableProperty]
    private int positionsGained;

    [ObservableProperty]
    private string licenseClass = string.Empty;

    [ObservableProperty]
    private string licenseColor = "#6B7280";

    [ObservableProperty]
    private int iRating;

    [ObservableProperty]
    private int iRatingGain;

    [ObservableProperty]
    private string carBrand = string.Empty;

    [ObservableProperty]
    private double bestLapTime;

    [ObservableProperty]
    private double gapToLeader;

    [ObservableProperty]
    private double interval;

    [ObservableProperty]
    private double lastLapTime;

    [ObservableProperty]
    private double delta;

    [ObservableProperty]
    private bool isPlayer;

    [ObservableProperty]
    private bool isInPit;
}
