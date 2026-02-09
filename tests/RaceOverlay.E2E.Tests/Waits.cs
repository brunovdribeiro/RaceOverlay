namespace RaceOverlay.E2E.Tests;

/// <summary>
/// Centralized wait/timeout constants for E2E tests.
/// </summary>
public static class Waits
{
    public const int ConfigChangeMs = 200;
    public const int UISettleMs = 500;
    public const int AppLifecycleMs = 1000;
    public const int ConfigRestoreMs = 2000;

    public static readonly TimeSpan OverlayTimeout = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan RetryInterval = TimeSpan.FromMilliseconds(250);
    public static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(15);
}
