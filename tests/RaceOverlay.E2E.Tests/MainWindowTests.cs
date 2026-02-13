using FlaUI.Core.Definitions;

namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class MainWindowTests : TestBase
{
    public MainWindowTests(AppFixture fixture) : base(fixture) { }

    [Fact]
    public void MainWindow_ShouldHaveCorrectTitle()
    {
        var window = Fixture.GetMainWindow();
        Assert.Contains("RaceOverlay", window.Title);
    }

    [Fact]
    public void MainWindow_ShouldShowWidgetLibrary()
    {
        var window = Fixture.GetMainWindow();
        Assert.NotNull(window.FindFirstDescendant(cf => cf.ByAutomationId("WidgetLibraryList")));
    }

    [Fact]
    public void MainWindow_ShouldShowStartLayoutButton()
    {
        var window = Fixture.GetMainWindow();
        Assert.NotNull(window.FindFirstDescendant(cf => cf.ByAutomationId("StartLayoutButton")));
    }

    [Fact]
    public void MainWindow_ShouldShowToggleSetupModeButton()
    {
        var window = Fixture.GetMainWindow();
        Assert.NotNull(window.FindFirstDescendant(cf => cf.ByAutomationId("ToggleSetupModeButton")));
    }

    [Fact]
    public void MainWindow_ShouldStartWithNoActiveWidgets()
    {
        var window = Fixture.GetMainWindow();
        Assert.NotNull(window.FindFirstDescendant(cf =>
            cf.ByText("Toggle widgets on from the library to activate them")));
    }

    [Fact]
    public void MainWindow_ShouldShowConfigPlaceholder()
    {
        var window = Fixture.GetMainWindow();
        Assert.NotNull(window.FindFirstDescendant(cf =>
            cf.ByText("Select an active widget to configure")));
    }

    [Fact]
    public void WidgetLibrary_ShouldContainExpectedWidgets()
    {
        var window = Fixture.GetMainWindow();
        foreach (var name in WidgetNames.All)
            Assert.NotNull(window.FindFirstDescendant(cf => cf.ByText(name)));
    }

    [Fact]
    public void WidgetLibrary_AllTogglesShouldStartOff()
    {
        foreach (var name in WidgetNames.All)
            Assert.Null(Fixture.FindOverlayWindow(name));
    }

    [Fact]
    public void WidgetLibrary_ShouldShowEightItems()
    {
        var library = Fixture.GetMainWindow()
            .FindFirstDescendant(cf => cf.ByAutomationId("WidgetLibraryList"));
        Assert.NotNull(library);

        var toggles = library.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
        Assert.Equal(8, toggles.Length);
    }
}
