using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RaceOverlay.Core.Providers;

namespace RaceOverlay.Core.Services;

/// <summary>
/// Detects which racing game is running and manages provider switching.
/// When no game is detected, runs in demo mode.
/// Once a game is detected, stops detection until the game disconnects.
/// </summary>
public class GameDetectionService : IGameDetectionService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GameDetectionService> _logger;
    private readonly List<IGameProvider> _providers = new();
    private readonly DemoTelemetryService _demoService;
    private IGameProvider? _activeProvider;
    private ILiveTelemetryService? _activeTelemetryService;
    private CancellationTokenSource? _cts;
    private Task? _detectionTask;
    private readonly object _lock = new();

    public string? ActiveGameId => _activeProvider?.GameId;
    public bool IsGameRunning => _activeProvider != null && _activeProvider.IsGameRunning();
    public bool IsDemoMode => _activeProvider == null;
    public ILiveTelemetryService ActiveTelemetryService => _activeTelemetryService ?? _demoService;

    public event EventHandler<string>? GameDetected;
    public event EventHandler? GameDisconnected;
    public event EventHandler? DemoModeActivated;

    public GameDetectionService(
        IServiceProvider serviceProvider,
        ILogger<GameDetectionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _demoService = new DemoTelemetryService();
    }

    public void RegisterProvider(IGameProvider provider)
    {
        lock (_lock)
        {
            _providers.Add(provider);
            _logger.LogInformation("Registered game provider: {GameId}", provider.GameId);
        }
    }

    public void Start()
    {
        if (_detectionTask != null)
        {
            _logger.LogWarning("Game detection already started");
            return;
        }

        _logger.LogInformation("Starting game detection service");
        _cts = new CancellationTokenSource();
        _detectionTask = Task.Run(async () => await DetectionLoopAsync(_cts.Token), _cts.Token);
    }

    public void Stop()
    {
        _logger.LogInformation("Stopping game detection service");
        _cts?.Cancel();
        _detectionTask?.Wait(TimeSpan.FromSeconds(2));
        _detectionTask = null;
        _cts?.Dispose();
        _cts = null;

        lock (_lock)
        {
            if (_activeProvider != null)
            {
                _activeTelemetryService?.Stop();
                _activeProvider = null;
                _activeTelemetryService = null;
            }
        }
    }

    private async Task DetectionLoopAsync(CancellationToken cancellationToken)
    {
        // Start in demo mode
        _logger.LogInformation("Starting in demo mode");
        _demoService.Start();
        DemoModeActivated?.Invoke(this, EventArgs.Empty);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // If we have an active provider, monitor its connection
                if (_activeProvider != null)
                {
                    if (!_activeProvider.IsGameRunning())
                    {
                        // Game disconnected
                        _logger.LogInformation("Game {GameId} disconnected", _activeProvider.GameId);
                        await DisconnectActiveGameAsync();
                    }

                    // While game is running, just wait and don't detect other games
                    await Task.Delay(1000, cancellationToken);
                }
                else
                {
                    // No active game, scan for available games
                    var detectedProvider = await DetectAvailableGameAsync();

                    if (detectedProvider != null)
                    {
                        // Found a game, connect to it
                        await ConnectToGameAsync(detectedProvider);
                    }
                    else
                    {
                        // No game found, stay in demo mode
                        await Task.Delay(2000, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in game detection loop");
                await Task.Delay(5000, cancellationToken);
            }
        }
    }

    private async Task<IGameProvider?> DetectAvailableGameAsync()
    {
        lock (_lock)
        {
            // Check each provider to see if its game is running
            foreach (var provider in _providers)
            {
                try
                {
                    if (provider.IsGameRunning())
                    {
                        _logger.LogInformation("Detected game: {GameId}", provider.GameId);
                        return provider;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking if {GameId} is running", provider.GameId);
                }
            }

            return null;
        }
    }

    private async Task ConnectToGameAsync(IGameProvider provider)
    {
        try
        {
            _logger.LogInformation("Connecting to {GameId}...", provider.GameId);

            // Stop demo mode
            _demoService.Stop();

            // Get the telemetry service from the provider
            var telemetryService = provider.TelemetryService;

            // Start the telemetry service
            telemetryService.Start();

            // Subscribe to disconnect events
            telemetryService.OnDisconnected += HandleProviderDisconnected;

            lock (_lock)
            {
                _activeProvider = provider;
                _activeTelemetryService = telemetryService;
            }

            _logger.LogInformation("Successfully connected to {GameId}", provider.GameId);
            GameDetected?.Invoke(this, provider.GameId);

            await provider.StartAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to {GameId}", provider.GameId);
        }
    }

    private async Task DisconnectActiveGameAsync()
    {
        IGameProvider? providerToDisconnect;
        ILiveTelemetryService? serviceToStop;

        lock (_lock)
        {
            providerToDisconnect = _activeProvider;
            serviceToStop = _activeTelemetryService;
            _activeProvider = null;
            _activeTelemetryService = null;
        }

        if (providerToDisconnect != null)
        {
            try
            {
                await providerToDisconnect.StopAsync();
                serviceToStop?.Stop();

                if (serviceToStop != null)
                {
                    serviceToStop.OnDisconnected -= HandleProviderDisconnected;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from {GameId}", providerToDisconnect.GameId);
            }
        }

        GameDisconnected?.Invoke(this, EventArgs.Empty);
        _logger.LogInformation("Entering demo mode");
        _demoService.Start();
        DemoModeActivated?.Invoke(this, EventArgs.Empty);
    }

    private void HandleProviderDisconnected()
    {
        // This will be picked up by the detection loop
        _logger.LogInformation("Provider disconnected event received");
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
