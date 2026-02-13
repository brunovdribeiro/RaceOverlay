using FlaUI.Core.Tools;

namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class ActiveWidgetCardTests : TestBase
{
    public ActiveWidgetCardTests(AppFixture fixture) : base(fixture) { }

    [Fact]
    public void ToggleWidgetOn_ShouldCreateActiveWidgetCard()
    {
        using var _ = Fixture.ActivateWidgets(WidgetNames.FuelCalculator);

        var card = Fixture.FindCardInCenterPanel(Fixture.GetMainWindow(), WidgetNames.FuelCalculator);
        Assert.NotNull(card);
    }

    [Fact]
    public void ToggleWidgetOff_ShouldRemoveActiveWidgetCard()
    {
        var mainWindow = Fixture.GetMainWindow();

        Fixture.ToggleWidgetOn(mainWindow, WidgetNames.Inputs);
        Fixture.ToggleWidgetOff(mainWindow, WidgetNames.Inputs);
        Thread.Sleep(Waits.UISettleMs);

        Assert.NotNull(mainWindow.FindFirstDescendant(cf =>
            cf.ByText("Toggle widgets on from the library to activate them")));
    }

    [Fact]
    public void ClickActiveWidgetCard_ShouldShowConfigPanel()
    {
        using var _ = Fixture.ActivateAndSelect(WidgetNames.FuelCalculator);

        var mainWindows = Fixture.GetMainWindow();
        
        
        Assert.NotNull(mainWindows.FindFirstDescendant(cf => cf.ByText("FUEL SETTINGS")));
    }

    [Fact]
    public void ClickDifferentCard_ShouldSwitchConfigPanel()
    {
        using var _ = Fixture.ActivateWidgets(WidgetNames.FuelCalculator, WidgetNames.Weather);
        var mainWindow = Fixture.GetMainWindow();

        Fixture.ClickActiveWidgetCard(mainWindow, WidgetNames.FuelCalculator);
        Assert.NotNull(mainWindow.FindFirstDescendant(cf => cf.ByText("FUEL SETTINGS")));

        Fixture.ClickActiveWidgetCard(mainWindow, WidgetNames.Weather);
        Assert.NotNull(mainWindow.FindFirstDescendant(cf => cf.ByText("WEATHER SETTINGS")));
    }

    [Fact]
    public void MultipleWidgetsActive_ShouldShowMultipleCards()
    {
        using var _ = Fixture.ActivateWidgets(
            WidgetNames.RelativeOverlay, WidgetNames.Standings, WidgetNames.LapTimer);
        var mainWindow = Fixture.GetMainWindow();

        var relCard = Retry.WhileNull(
            () => Fixture.FindCardInCenterPanel(mainWindow, WidgetNames.RelativeOverlay),
            timeout: Waits.OverlayTimeout, interval: Waits.RetryInterval);
        var standingsCard = Retry.WhileNull(
            () => Fixture.FindCardInCenterPanel(mainWindow, WidgetNames.Standings),
            timeout: Waits.OverlayTimeout, interval: Waits.RetryInterval);
        var lapTimerCard = Retry.WhileNull(
            () => Fixture.FindCardInCenterPanel(mainWindow, WidgetNames.LapTimer),
            timeout: Waits.OverlayTimeout, interval: Waits.RetryInterval);

        Assert.NotNull(relCard.Result);
        Assert.NotNull(standingsCard.Result);
        Assert.NotNull(lapTimerCard.Result);
    }
}
