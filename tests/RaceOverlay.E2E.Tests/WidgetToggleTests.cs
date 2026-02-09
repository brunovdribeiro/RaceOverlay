using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;

namespace RaceOverlay.E2E.Tests;

public class WidgetToggleTests : IClassFixture<AppFixture>
{
    private readonly AppFixture _fixture;

    public WidgetToggleTests(AppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void RelativeOverlay_EnableToggle_ShowsOverlayWindow()
    {
        var mainWindow = _fixture.GetMainWindow();

        // Find the "Relative Overlay" label in the widget library
        var label = mainWindow.FindFirstDescendant(cf => cf.ByText("Relative Overlay"));
        Assert.NotNull(label);

        // The toggle is a sibling in the same DockPanel (the label's parent)
        var container = label.Parent;
        Assert.NotNull(container);
        var toggle = container.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));
        Assert.NotNull(toggle);

        // Enable the widget
        toggle.Click();

        // Wait for the overlay window titled "Relative Overlay" to appear
        var overlayFound = Retry.WhileFalse(
            () => FindOverlayWindow("Relative Overlay") != null,
            timeout: TimeSpan.FromSeconds(5),
            interval: TimeSpan.FromMilliseconds(250));

        var overlayWindow = FindOverlayWindow("Relative Overlay");
        Assert.NotNull(overlayWindow);

        // Disable the widget
        toggle.Click();

        // Wait for the overlay window to disappear
        var overlayGone = Retry.WhileTrue(
            () => FindOverlayWindow("Relative Overlay") != null,
            timeout: TimeSpan.FromSeconds(5),
            interval: TimeSpan.FromMilliseconds(250));

        var overlayAfterDisable = FindOverlayWindow("Relative Overlay");
        Assert.Null(overlayAfterDisable);
    }

    private Window? FindOverlayWindow(string title)
    {
        // Overlay windows are top-level windows owned by the same process
        var allWindows = _fixture.App.GetAllTopLevelWindows(_fixture.Automation);
        return allWindows.FirstOrDefault(w => w.Title == title);
    }
}
