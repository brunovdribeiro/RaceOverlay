namespace RaceOverlay.Core.Services;

/// <summary>
/// Service for detecting which racing game is currently running
/// </summary>
public interface IGameDetectionService
{
    /// <summary>
    /// Currently active game provider, or null if in demo mode
    /// </summary>
    string? ActiveGameId { get; }

    /// <summary>
    /// Whether a game is currently detected and running
    /// </summary>
    bool IsGameRunning { get; }

    /// <summary>
    /// Whether the service is in demo mode (no game detected)
    /// </summary>
    bool IsDemoMode { get; }

    /// <summary>
    /// Event fired when a game is detected and connected
    /// </summary>
    event EventHandler<string>? GameDetected;

    /// <summary>
    /// Event fired when the current game disconnects
    /// </summary>
    event EventHandler? GameDisconnected;

    /// <summary>
    /// Event fired when switching to demo mode
    /// </summary>
    event EventHandler? DemoModeActivated;

    /// <summary>
    /// Start the game detection service
    /// </summary>
    void Start();

    /// <summary>
    /// Stop the game detection service
    /// </summary>
    void Stop();
}
