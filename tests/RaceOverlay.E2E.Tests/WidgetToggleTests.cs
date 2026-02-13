using FlaUI.Core.Tools;

namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class WidgetToggleTests : TestBase
{
    public WidgetToggleTests(AppFixture fixture) : base(fixture) { }

    [Theory]
    [InlineData(WidgetNames.RelativeOverlay)]
    [InlineData(WidgetNames.FuelCalculator)]
    [InlineData(WidgetNames.Inputs)]
    // [InlineData(WidgetNames.InputTrace)]
    // [InlineData(WidgetNames.Standings)]
    [InlineData(WidgetNames.LapTimer)]
    [InlineData(WidgetNames.TrackMap)]
    [InlineData(WidgetNames.Weather)]
    public void EnableToggle_ShowsOverlayWindow_DisableToggle_HidesIt(string widgetName)
    {
        var mainWindow = Fixture.GetMainWindowIncludingHidden();

        // Use Retry to handle stale UI tree after window close/restore cycles
        var result = Retry.WhileNull(
            () => Fixture.FindWidgetToggle(Fixture.GetMainWindow(), widgetName),
            timeout: Waits.OverlayTimeout,
            interval: Waits.RetryInterval);
        var toggle = result.Result;
        Assert.NotNull(toggle);

        // Enable
        toggle.Click();
        Retry.WhileFalse(
            () => Fixture.FindOverlayWindow(widgetName) != null,
            timeout: Waits.OverlayTimeout,
            interval: Waits.RetryInterval);
        Assert.NotNull(Fixture.FindOverlayWindow(widgetName));

        // Disable
        toggle.Click();
        Retry.WhileTrue(
            () => Fixture.FindOverlayWindow(widgetName) != null,
            timeout: Waits.OverlayTimeout,
            interval: Waits.RetryInterval);
        Assert.Null(Fixture.FindOverlayWindow(widgetName));
    }
}
