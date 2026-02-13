using FlaUI.Core;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace RaceOverlay.E2E.Tests;

/// <summary>
/// System tray tests run outside the shared fixture because closing the main window
/// calls WPF's Hide(), which cannot be reversed from outside the process.
/// Each test manages its own app lifecycle to avoid poisoning other tests.
/// </summary>
public class SystemTrayTests : IDisposable
{
    private Application? _app;
    private UIA3Automation? _automation;

    public SystemTrayTests()
    {
        AppFixture.BackupConfig();
        AppFixture.DeleteConfig();
    }

    public void Dispose()
    {
        _app?.Kill();
        _automation?.Dispose();
        AppFixture.RestoreConfig();
        GC.SuppressFinalize(this);
    }

    private FlaUI.Core.AutomationElements.Window LaunchAndGetMainWindow()
    {
        _app?.Kill();
        _automation?.Dispose();

        var exePath = AppFixture.FindAppExecutable();
        _app = Application.Launch(exePath);
        _automation = new UIA3Automation();

        return _app.GetMainWindow(_automation, Waits.StartupTimeout)
            ?? throw new InvalidOperationException("Main window did not appear.");
    }

    [Fact]
    public void CloseMainWindow_ShouldMinimizeToTray()
    {
        var mainWindow = LaunchAndGetMainWindow();

        mainWindow.Close();
        Thread.Sleep(Waits.AppLifecycleMs);

        Assert.False(_app!.HasExited, "App should not exit when closing main window");
    }

    [Fact(Skip = "Win32 NotifyIcon is not accessible via UI Automation — requires manual testing")]
    public void TrayIcon_DoubleClick_RestoresMainWindow()
    {
    }

    [Fact(Skip = "Win32 NotifyIcon context menu is not accessible via UI Automation — requires manual testing")]
    public void TrayMenu_Exit_ShutsDownApp()
    {
    }
}
