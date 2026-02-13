namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class StartLayoutTests : TestBase
{
    public StartLayoutTests(AppFixture fixture) : base(fixture) { }

    [Fact]
    public void StartLayout_MinimizesMainWindow()
    {
        using var _ = Fixture.ActivateWidgets(WidgetNames.Weather);
        var mainWindow = Fixture.GetMainWindow();

        var startButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("StartLayoutButton"));
        Assert.NotNull(startButton);

        startButton.Click();
        Thread.Sleep(Waits.AppLifecycleMs);

        mainWindow = Fixture.GetMainWindow();
        Assert.True(Fixture.IsWindowMinimized(mainWindow),
            "Main window should be minimized after Start Layout");

        // Restore window for other tests
        mainWindow.Patterns.Window.Pattern.SetWindowVisualState(
            FlaUI.Core.Definitions.WindowVisualState.Normal);
        Thread.Sleep(Waits.UISettleMs);
    }

    [Fact]
    public void StartLayout_OverlayWindowsRemainVisible()
    {
        using var _ = Fixture.ActivateWidgets(WidgetNames.LapTimer);
        var mainWindow = Fixture.GetMainWindow();

        var startButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("StartLayoutButton"));
        Assert.NotNull(startButton);

        startButton.Click();
        Thread.Sleep(Waits.AppLifecycleMs);

        Assert.NotNull(Fixture.FindOverlayWindow(WidgetNames.LapTimer));

        // Restore main window
        mainWindow = Fixture.GetMainWindow();
        mainWindow.Patterns.Window.Pattern.SetWindowVisualState(
            FlaUI.Core.Definitions.WindowVisualState.Normal);
        Thread.Sleep(Waits.UISettleMs);
    }
}
