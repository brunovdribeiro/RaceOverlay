using System.IO;
using FlaUI.Core;
using FlaUI.UIA3;

namespace RaceOverlay.E2E.Tests;

/// <summary>
/// Shared fixture that launches the app once per test collection and tears it down after.
/// Backs up and removes the user's config so the app starts in a clean state,
/// then restores it on disposal.
/// </summary>
public class AppFixture : IDisposable
{
    private const int StartupTimeoutMs = 15_000;

    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RaceOverlay");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");
    private static readonly string ConfigBackupPath = ConfigPath + ".e2e-backup";

    public Application App { get; }
    public UIA3Automation Automation { get; }

    public AppFixture()
    {
        BackupConfig();

        var exePath = FindAppExecutable();
        App = Application.Launch(exePath);
        Automation = new UIA3Automation();

        // Wait for the main window to appear
        var mainWindow = App.GetMainWindow(Automation, TimeSpan.FromMilliseconds(StartupTimeoutMs));
        if (mainWindow == null)
            throw new InvalidOperationException("Main window did not appear within the timeout period.");
    }

    public FlaUI.Core.AutomationElements.Window GetMainWindow()
    {
        return App.GetMainWindow(Automation, TimeSpan.FromSeconds(5))
            ?? throw new InvalidOperationException("Main window not found.");
    }

    public void Dispose()
    {
        // Kill the process since the window close is intercepted by minimize-to-tray
        App?.Kill();
        Automation?.Dispose();
        RestoreConfig();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Moves the user's config aside so the app starts with no saved widgets.
    /// </summary>
    private static void BackupConfig()
    {
        if (File.Exists(ConfigPath))
        {
            File.Copy(ConfigPath, ConfigBackupPath, overwrite: true);
            File.Delete(ConfigPath);
        }
    }

    /// <summary>
    /// Restores the original config after tests complete.
    /// </summary>
    private static void RestoreConfig()
    {
        if (File.Exists(ConfigBackupPath))
        {
            File.Copy(ConfigBackupPath, ConfigPath, overwrite: true);
            File.Delete(ConfigBackupPath);
        }
    }

    private static string FindAppExecutable()
    {
        var baseDir = AppContext.BaseDirectory;
        var repoRoot = FindRepoRoot(baseDir);

        var configurations = new[] { "Debug", "Release" };
        foreach (var config in configurations)
        {
            var exePath = Path.Combine(repoRoot, "src", "RaceOverlay.App", "bin", config, "net10.0-windows", "RaceOverlay.App.exe");
            if (File.Exists(exePath))
                return exePath;
        }

        throw new FileNotFoundException(
            "Could not find RaceOverlay.App.exe. Build the app project first: dotnet build src/RaceOverlay.App/RaceOverlay.App.csproj");
    }

    private static string FindRepoRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root (.git directory).");
    }
}
