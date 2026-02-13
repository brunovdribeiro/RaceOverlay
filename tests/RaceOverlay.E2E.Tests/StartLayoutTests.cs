namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class StartLayoutTests(AppFixture fixture)
{
    [Fact]
    public void StartLayout_MinimizesMainWindow()
    {
        using var _ = fixture.ActivateWidgets(WidgetNames.Weather);
        var mainWindow = fixture.GetMainWindow();

        var startButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("StartLayoutButton"));
        Assert.NotNull(startButton);

        startButton.Click();
        Thread.Sleep(Waits.AppLifecycleMs);

        mainWindow = fixture.GetMainWindow();
        Assert.True(fixture.IsWindowMinimized(mainWindow),
            "Main window should be minimized after Start Layout");

        // Restore window for other tests
        mainWindow.Patterns.Window.Pattern.SetWindowVisualState(
            FlaUI.Core.Definitions.WindowVisualState.Normal);
        Thread.Sleep(Waits.UISettleMs);
    }

    [Fact]
    public void StartLayout_OverlayWindowsRemainVisible()
    {
        using var _ = fixture.ActivateWidgets(WidgetNames.LapTimer);
        var mainWindow = fixture.GetMainWindow();

        var startButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("StartLayoutButton"));
        Assert.NotNull(startButton);

        startButton.Click();
        Thread.Sleep(Waits.AppLifecycleMs);

        Assert.NotNull(fixture.FindOverlayWindow(WidgetNames.LapTimer));

        // Restore main window
        mainWindow = fixture.GetMainWindow();
        mainWindow.Patterns.Window.Pattern.SetWindowVisualState(
            FlaUI.Core.Definitions.WindowVisualState.Normal);
        Thread.Sleep(Waits.UISettleMs);
    }
}
