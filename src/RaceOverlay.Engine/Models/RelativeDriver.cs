namespace RaceOverlay.Engine.Models;

/// <summary>
/// Represents a driver relative to the player's position on the track.
/// Used for the Relative Overlay widget to display nearby drivers.
/// </summary>
public class RelativeDriver
{
    /// <summary>
    /// Unique identifier for this driver instance.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Race position of the driver (1-based).
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Car number/identifier.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Driver name.
    /// </summary>
    public string DriverName { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle class (e.g., "LMP2", "GT3"). Used for visual styling.
    /// </summary>
    public string VehicleClass { get; set; } = string.Empty;

    /// <summary>
    /// Class color for UI visualization (e.g., #D946EF for burgundy, #D97706 for gold).
    /// </summary>
    public string ClassColor { get; set; } = "#6B7280";

    /// <summary>
    /// Elo rating of the driver (1600-3500 typical range).
    /// </summary>
    public double EloRating { get; set; }

    /// <summary>
    /// Elo grade letter (S, A+, A, B+, B, C, D, etc.).
    /// </summary>
    public string EloGrade { get; set; } = "C";

    /// <summary>
    /// Elo grade color for UI display.
    /// </summary>
    public string EloGradeColor { get; set; } = "#6B7280";

    /// <summary>
    /// Current lap time in seconds.
    /// </summary>
    public double CurrentLapTime { get; set; }

    /// <summary>
    /// Best lap time in seconds.
    /// </summary>
    public double BestLapTime { get; set; }

    /// <summary>
    /// Delta from best lap in seconds (positive = slower than best).
    /// </summary>
    public double DeltaFromBest { get; set; }

    /// <summary>
    /// Gap/interval to the next driver ahead on track (in meters, negative = behind).
    /// </summary>
    public double GapToNextDriver { get; set; }

    /// <summary>
    /// Current stint laps completed.
    /// </summary>
    public int StintLapsCompleted { get; set; }

    /// <summary>
    /// Total laps planned for the stint.
    /// </summary>
    public int StintLapsTotal { get; set; }

    /// <summary>
    /// Time spent in current stint (formatted string, e.g., "45:23").
    /// </summary>
    public string StintTime { get; set; } = "00:00";

    /// <summary>
    /// Whether the driver is in the pit lane.
    /// </summary>
    public bool IsInPit { get; set; }

    /// <summary>
    /// Driver status flag (e.g., "OUT", "DNF", "DAMAGE"). Null if none.
    /// </summary>
    public string? StatusFlag { get; set; }

    /// <summary>
    /// Whether the driver has damage (affects reliability/pit stops).
    /// </summary>
    public bool HasDamage { get; set; }

    /// <summary>
    /// Distance on track from start/finish line in meters.
    /// Used to calculate relative positioning.
    /// </summary>
    public double TrackDistanceMeters { get; set; }

    /// <summary>
    /// Relative position indicator: -1 = ahead, 0 = player, 1 = behind.
    /// </summary>
    public int RelativePosition { get; set; }
}
