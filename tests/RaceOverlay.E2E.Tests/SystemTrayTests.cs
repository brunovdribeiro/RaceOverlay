namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class SystemTrayTests(AppFixture fixture)
{
    [Fact]
    public void CloseMainWindow_ShouldMinimizeToTray()
    {
        var mainWindow = fixture.GetMainWindow();

        mainWindow.Close();
        Thread.Sleep(Waits.AppLifecycleMs);

        Assert.False(fixture.App.HasExited, "App should not exit when closing main window");

        // Restore window for other tests (get hidden window and show it)
        mainWindow = fixture.GetMainWindowIncludingHidden();
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
