namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class SystemTrayTests
{
    private readonly AppFixture _fixture;

    public SystemTrayTests(AppFixture fixture) => _fixture = fixture;

    [Fact]
    public void CloseMainWindow_ShouldMinimizeToTray()
    {
        var mainWindow = _fixture.GetMainWindow();

        mainWindow.Close();
        Thread.Sleep(Waits.AppLifecycleMs);

        Assert.False(_fixture.App.HasExited, "App should not exit when closing main window");

        // Restore window for other tests
        mainWindow = _fixture.GetMainWindow();
        mainWindow.Patterns.Window.Pattern.SetWindowVisualState(
            FlaUI.Core.Definitions.WindowVisualState.Normal);
        Thread.Sleep(Waits.UISettleMs);
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
