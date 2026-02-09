using FlaUI.Core.Definitions;

namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class MainWindowTests
{
    private readonly AppFixture _fixture;

    public MainWindowTests(AppFixture fixture) => _fixture = fixture;

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
        Assert.NotNull(window.FindFirstDescendant(cf => cf.ByAutomationId("WidgetLibraryList")));
    }

    [Fact]
    public void MainWindow_ShouldShowStartLayoutButton()
    {
        var window = _fixture.GetMainWindow();
        Assert.NotNull(window.FindFirstDescendant(cf => cf.ByAutomationId("StartLayoutButton")));
    }

    [Fact]
    public void MainWindow_ShouldShowToggleSetupModeButton()
    {
        var window = _fixture.GetMainWindow();
        Assert.NotNull(window.FindFirstDescendant(cf => cf.ByAutomationId("ToggleSetupModeButton")));
    }

    [Fact]
    public void MainWindow_ShouldOpenCenteredOnScreen()
    {
        var bounds = _fixture.GetMainWindow().BoundingRectangle;
        Assert.True(bounds.Left > 0, "Window should not be at x=0");
        Assert.True(bounds.Top > 0, "Window should not be at y=0");
    }

    [Fact]
    public void MainWindow_ShouldStartWithNoActiveWidgets()
    {
        var window = _fixture.GetMainWindow();
        Assert.NotNull(window.FindFirstDescendant(cf =>
            cf.ByText("Toggle widgets on from the library to activate them")));
    }

    [Fact]
    public void MainWindow_ShouldShowConfigPlaceholder()
    {
        var window = _fixture.GetMainWindow();
        Assert.NotNull(window.FindFirstDescendant(cf =>
            cf.ByText("Select an active widget to configure")));
    }

    [Fact]
    public void WidgetLibrary_ShouldContainExpectedWidgets()
    {
        var window = _fixture.GetMainWindow();
        foreach (var name in WidgetNames.All)
            Assert.NotNull(window.FindFirstDescendant(cf => cf.ByText(name)));
    }

    [Fact]
    public void WidgetLibrary_AllTogglesShouldStartOff()
    {
        foreach (var name in WidgetNames.All)
            Assert.Null(_fixture.FindOverlayWindow(name));
    }

    [Fact]
    public void WidgetLibrary_ShouldShowEightItems()
    {
        var library = _fixture.GetMainWindow()
            .FindFirstDescendant(cf => cf.ByAutomationId("WidgetLibraryList"));
        Assert.NotNull(library);

        var toggles = library.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
        Assert.Equal(8, toggles.Length);
    }
}
