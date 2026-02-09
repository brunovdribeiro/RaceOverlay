namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class ActiveWidgetCardTests
{
    private readonly AppFixture _fixture;

    public ActiveWidgetCardTests(AppFixture fixture) => _fixture = fixture;

    [Fact]
    public void ToggleWidgetOn_ShouldCreateActiveWidgetCard()
    {
        using var _ = _fixture.ActivateWidgets(WidgetNames.FuelCalculator);

        var card = _fixture.FindCardInCenterPanel(_fixture.GetMainWindow(), WidgetNames.FuelCalculator);
        Assert.NotNull(card);
    }

    [Fact]
    public void ToggleWidgetOff_ShouldRemoveActiveWidgetCard()
    {
        var mainWindow = _fixture.GetMainWindow();

        _fixture.ToggleWidgetOn(mainWindow, WidgetNames.Inputs);
        _fixture.ToggleWidgetOff(mainWindow, WidgetNames.Inputs);
        Thread.Sleep(Waits.UISettleMs);

        Assert.NotNull(mainWindow.FindFirstDescendant(cf =>
            cf.ByText("Toggle widgets on from the library to activate them")));
    }

    [Fact]
    public void ClickActiveWidgetCard_ShouldShowConfigPanel()
    {
        using var _ = _fixture.ActivateAndSelect(WidgetNames.FuelCalculator);

        Assert.NotNull(_fixture.GetMainWindow()
            .FindFirstDescendant(cf => cf.ByText("FUEL SETTINGS")));
    }

    [Fact]
    public void ClickDifferentCard_ShouldSwitchConfigPanel()
    {
        using var _ = _fixture.ActivateWidgets(WidgetNames.FuelCalculator, WidgetNames.Weather);
        var mainWindow = _fixture.GetMainWindow();

        _fixture.ClickActiveWidgetCard(mainWindow, WidgetNames.FuelCalculator);
        Assert.NotNull(mainWindow.FindFirstDescendant(cf => cf.ByText("FUEL SETTINGS")));

        _fixture.ClickActiveWidgetCard(mainWindow, WidgetNames.Weather);
        Assert.NotNull(mainWindow.FindFirstDescendant(cf => cf.ByText("WEATHER SETTINGS")));
    }

    [Fact]
    public void MultipleWidgetsActive_ShouldShowMultipleCards()
    {
        using var _ = _fixture.ActivateWidgets(
            WidgetNames.RelativeOverlay, WidgetNames.Standings, WidgetNames.LapTimer);
        var mainWindow = _fixture.GetMainWindow();

        Assert.NotNull(_fixture.FindCardInCenterPanel(mainWindow, WidgetNames.RelativeOverlay));
        Assert.NotNull(_fixture.FindCardInCenterPanel(mainWindow, WidgetNames.Standings));
        Assert.NotNull(_fixture.FindCardInCenterPanel(mainWindow, WidgetNames.LapTimer));
    }
}
