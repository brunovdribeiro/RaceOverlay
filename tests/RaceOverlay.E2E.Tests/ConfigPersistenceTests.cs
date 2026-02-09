using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace RaceOverlay.E2E.Tests;

/// <summary>
/// Config persistence tests run outside the shared fixture because they need
/// to restart the app between steps. Each test manages its own app lifecycle.
/// </summary>
public class ConfigPersistenceTests : IDisposable
{
    private Application? _app;
    private UIA3Automation? _automation;

    public ConfigPersistenceTests()
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

    private Window LaunchAndGetMainWindow()
    {
        _app?.Kill();
        _automation?.Dispose();

        var exePath = AppFixture.FindAppExecutable();
        _app = Application.Launch(exePath);
        _automation = new UIA3Automation();

        return _app.GetMainWindow(_automation, Waits.StartupTimeout)
            ?? throw new InvalidOperationException("Main window did not appear.");
    }

    private Window? FindOverlayWindow(string title)
    {
        var allWindows = _app!.GetAllTopLevelWindows(_automation!);
        return allWindows.FirstOrDefault(w => w.Title == title);
    }

    private AutomationElement? FindWidgetToggle(Window mainWindow, string widgetName)
    {
        var label = mainWindow.FindFirstDescendant(cf => cf.ByText(widgetName));
        if (label?.Parent == null) return null;
        return label.Parent.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));
    }

    [Fact]
    public void EnableWidget_CloseApp_ReopenApp_WidgetRestored()
    {
        // First launch — enable a widget
        var mainWindow = LaunchAndGetMainWindow();

        var toggle = FindWidgetToggle(mainWindow, WidgetNames.Standings);
        Assert.NotNull(toggle);
        toggle.Click();

        Retry.WhileFalse(
            () => FindOverlayWindow(WidgetNames.Standings) != null,
            timeout: Waits.OverlayTimeout,
            interval: Waits.RetryInterval);

        Assert.NotNull(FindOverlayWindow(WidgetNames.Standings));

        // Kill the app (saves config on widget toggle)
        _app!.Kill();
        Thread.Sleep(Waits.AppLifecycleMs);

        // Second launch — widget should be restored
        mainWindow = LaunchAndGetMainWindow();
        Thread.Sleep(Waits.ConfigRestoreMs);

        Assert.NotNull(FindOverlayWindow(WidgetNames.Standings));
    }

    [Fact]
    public void NoConfigFile_StartsClean()
    {
        AppFixture.DeleteConfig();

        var mainWindow = LaunchAndGetMainWindow();
        Thread.Sleep(Waits.AppLifecycleMs);

        // No overlay windows should be present
        var allWindows = _app!.GetAllTopLevelWindows(_automation!);
        var overlays = allWindows.Where(w => w.Title != mainWindow.Title).ToArray();
        Assert.Empty(overlays);

        Assert.NotNull(mainWindow.FindFirstDescendant(cf =>
            cf.ByText("Toggle widgets on from the library to activate them")));
    }

    [Fact]
    public void CorruptConfigFile_StartsClean()
    {
        Directory.CreateDirectory(AppFixture.ConfigDir);
        File.WriteAllText(AppFixture.ConfigPath, "{{{{not valid json!!");

        var mainWindow = LaunchAndGetMainWindow();
        Thread.Sleep(Waits.AppLifecycleMs);

        var allWindows = _app!.GetAllTopLevelWindows(_automation!);
        var overlays = allWindows.Where(w => w.Title != mainWindow.Title).ToArray();
        Assert.Empty(overlays);
    }
}
