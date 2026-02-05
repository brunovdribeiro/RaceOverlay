using CommunityToolkit.Mvvm.ComponentModel;

namespace RaceOverlay.Engine.Models;

/// <summary>
/// Represents a driver relative to the player's position on the track.
/// Uses ObservableObject so WPF detects property changes when the update loop mutates driver data.
/// </summary>
public partial class RelativeDriver : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private int position;

    [ObservableProperty]
    private string number = string.Empty;

    [ObservableProperty]
    private string driverName = string.Empty;

    [ObservableProperty]
    private string vehicleClass = string.Empty;

    [ObservableProperty]
    private string classColor = "#6B7280";

    [ObservableProperty]
    private double eloRating;

    [ObservableProperty]
    private string eloGrade = "C";

    [ObservableProperty]
    private string eloGradeColor = "#6B7280";

    [ObservableProperty]
    private double currentLapTime;

    [ObservableProperty]
    private double bestLapTime;

    [ObservableProperty]
    private double deltaFromBest;

    [ObservableProperty]
    private double gapToNextDriver;

    [ObservableProperty]
    private int stintLapsCompleted;

    [ObservableProperty]
    private int stintLapsTotal;

    [ObservableProperty]
    private string stintTime = "00:00";

    [ObservableProperty]
    private bool isInPit;

    [ObservableProperty]
    private string? statusFlag;

    [ObservableProperty]
    private bool hasDamage;

    [ObservableProperty]
    private double trackDistanceMeters;

    [ObservableProperty]
    private int relativePosition;
}
