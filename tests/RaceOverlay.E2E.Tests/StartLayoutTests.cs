namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class StartLayoutTests
{
    private readonly AppFixture _fixture;

    public StartLayoutTests(AppFixture fixture) => _fixture = fixture;

    [Fact]
    public void StartLayout_MinimizesMainWindow()
    {
        using var _ = _fixture.ActivateWidgets(WidgetNames.Weather);
        var mainWindow = _fixture.GetMainWindow();

        var startButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("StartLayoutButton"));
        Assert.NotNull(startButton);

        startButton.Click();
        Thread.Sleep(Waits.AppLifecycleMs);

        mainWindow = _fixture.GetMainWindow();
        Assert.True(_fixture.IsWindowMinimized(mainWindow),
            "Main window should be minimized after Start Layout");

        // Restore window for other tests
        mainWindow.Patterns.Window.Pattern.SetWindowVisualState(
            FlaUI.Core.Definitions.WindowVisualState.Normal);
        Thread.Sleep(Waits.UISettleMs);
    }

    [Fact]
    public void StartLayout_OverlayWindowsRemainVisible()
    {
        using var _ = _fixture.ActivateWidgets(WidgetNames.LapTimer);
        var mainWindow = _fixture.GetMainWindow();

        var startButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("StartLayoutButton"));
        Assert.NotNull(startButton);

        startButton.Click();
        Thread.Sleep(Waits.AppLifecycleMs);

        Assert.NotNull(_fixture.FindOverlayWindow(WidgetNames.LapTimer));

        // Restore main window
        mainWindow = _fixture.GetMainWindow();
        mainWindow.Patterns.Window.Pattern.SetWindowVisualState(
            FlaUI.Core.Definitions.WindowVisualState.Normal);
        Thread.Sleep(Waits.UISettleMs);
    }
}
