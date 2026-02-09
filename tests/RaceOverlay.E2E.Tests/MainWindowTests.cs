using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace RaceOverlay.E2E.Tests;

public class MainWindowTests : IClassFixture<AppFixture>
{
    private readonly AppFixture _fixture;

    public MainWindowTests(AppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void MainWindow_ShouldHaveCorrectTitle()
    {
        var window = _fixture.GetMainWindow();
        Assert.Contains("RaceOverlay", window.Title);
    }

    [Fact]
    public void MainWindow_ShouldShowWidgetLibrary()
    {
        var window = _fixture.GetMainWindow();
        var widgetLibrary = window.FindFirstDescendant(cf => cf.ByAutomationId("WidgetLibraryList"));
        Assert.NotNull(widgetLibrary);
    }

    [Fact]
    public void MainWindow_ShouldShowStartLayoutButton()
    {
        var window = _fixture.GetMainWindow();
        var button = window.FindFirstDescendant(cf => cf.ByAutomationId("StartLayoutButton"));
        Assert.NotNull(button);
    }

    [Fact]
    public void MainWindow_ShouldShowToggleSetupModeButton()
    {
        var window = _fixture.GetMainWindow();
        var button = window.FindFirstDescendant(cf => cf.ByAutomationId("ToggleSetupModeButton"));
        Assert.NotNull(button);
    }

    [Fact]
    public void WidgetLibrary_ShouldContainExpectedWidgets()
    {
        var window = _fixture.GetMainWindow();

        var expectedWidgets = new[]
        {
            "Relative Overlay", "Fuel Calculator", "Inputs", "Input Trace",
            "Standings", "Lap Timer", "Track Map", "Weather"
        };

        foreach (var widgetName in expectedWidgets)
        {
            var element = window.FindFirstDescendant(cf => cf.ByText(widgetName));
            Assert.NotNull(element);
        }
    }
}
