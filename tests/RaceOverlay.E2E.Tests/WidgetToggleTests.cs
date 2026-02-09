using FlaUI.Core.Tools;

namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class WidgetToggleTests
{
    private readonly AppFixture _fixture;

    public WidgetToggleTests(AppFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData(WidgetNames.RelativeOverlay)]
    [InlineData(WidgetNames.FuelCalculator)]
    [InlineData(WidgetNames.Inputs)]
    [InlineData(WidgetNames.InputTrace)]
    [InlineData(WidgetNames.Standings)]
    [InlineData(WidgetNames.LapTimer)]
    [InlineData(WidgetNames.TrackMap)]
    [InlineData(WidgetNames.Weather)]
    public void EnableToggle_ShowsOverlayWindow_DisableToggle_HidesIt(string widgetName)
    {
        var mainWindow = _fixture.GetMainWindow();
        var toggle = _fixture.FindWidgetToggle(mainWindow, widgetName);
        Assert.NotNull(toggle);

        // Enable
        toggle.Click();
        Retry.WhileFalse(
            () => _fixture.FindOverlayWindow(widgetName) != null,
            timeout: Waits.OverlayTimeout,
            interval: Waits.RetryInterval);
        Assert.NotNull(_fixture.FindOverlayWindow(widgetName));

        // Disable
        toggle.Click();
        Retry.WhileTrue(
            () => _fixture.FindOverlayWindow(widgetName) != null,
            timeout: Waits.OverlayTimeout,
            interval: Waits.RetryInterval);
        Assert.Null(_fixture.FindOverlayWindow(widgetName));
    }
}
