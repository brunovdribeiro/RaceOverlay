using RaceOverlay.Core.Services;

namespace RaceOverlay.Core.Providers;

/// <summary>
/// Contract for game telemetry providers
/// </summary>
public interface IGameProvider
{
    /// <summary>
    /// Unique identifier for this game (e.g., "iRacing", "AssettoCorsa")
    /// </summary>
    string GameId { get; }

    /// <summary>
    /// Display name of the game
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// The telemetry service for this game
    /// </summary>
    ILiveTelemetryService TelemetryService { get; }

    /// <summary>
    /// Check if the game is currently running
    /// </summary>
    bool IsGameRunning();

    /// <summary>
    /// Start capturing telemetry data
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop capturing telemetry data
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Fired when new telemetry data is available
    /// </summary>
    event EventHandler<TelemetryData>? DataReceived;
}
