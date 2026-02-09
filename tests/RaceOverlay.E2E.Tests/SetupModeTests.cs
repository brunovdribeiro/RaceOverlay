using FlaUI.Core.Definitions;

namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class SetupModeTests
{
    private readonly AppFixture _fixture;

    public SetupModeTests(AppFixture fixture) => _fixture = fixture;

    [Fact]
    public void ToggleSetupMode_ButtonTextChanges()
    {
        var mainWindow = _fixture.GetMainWindow();
        var button = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ToggleSetupModeButton"));
        Assert.NotNull(button);

        var initialText = button.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text));

        // Click to enter setup mode
        button.Click();
        Thread.Sleep(Waits.UISettleMs);

        var afterEnterText = button.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text));
        Assert.NotNull(afterEnterText);

        // Click again to exit setup mode
        button.Click();
        Thread.Sleep(Waits.UISettleMs);
    }

    [Fact]
    public void SetupMode_OverlayWindowShowsDragIndicator()
    {
        using var _ = _fixture.ActivateWidgets(WidgetNames.FuelCalculator);
        var mainWindow = _fixture.GetMainWindow();

        var setupButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ToggleSetupModeButton"));
        Assert.NotNull(setupButton);

        // Enter setup mode
        setupButton.Click();
        Thread.Sleep(Waits.UISettleMs);

        var overlay = _fixture.FindOverlayWindow(WidgetNames.FuelCalculator);
        Assert.NotNull(overlay);
        Assert.NotNull(overlay.FindFirstDescendant(cf => cf.ByText("Drag Mode")));

        // Exit setup mode
        setupButton.Click();
        Thread.Sleep(Waits.UISettleMs);
    }

    [Fact]
    public void DragMode_CanMoveOverlayWindow()
    {
        using var _ = _fixture.ActivateWidgets(WidgetNames.Inputs);

        var overlay = _fixture.FindOverlayWindow(WidgetNames.Inputs);
        Assert.NotNull(overlay);
        var initialBounds = overlay.BoundingRectangle;

        // Enter setup mode
        var mainWindow = _fixture.GetMainWindow();
        var setupButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ToggleSetupModeButton"));
        setupButton!.Click();
        Thread.Sleep(Waits.UISettleMs);

        // Refresh overlay reference
        overlay = _fixture.FindOverlayWindow(WidgetNames.Inputs);
        Assert.NotNull(overlay);

        // Simulate drag
        var center = overlay.GetClickablePoint();
        FlaUI.Core.Input.Mouse.MoveTo(center);
        Thread.Sleep(100);
        FlaUI.Core.Input.Mouse.Down(FlaUI.Core.Input.MouseButton.Left);
        Thread.Sleep(100);
        FlaUI.Core.Input.Mouse.MoveTo(new System.Drawing.Point(
            (int)center.X + 100,
            (int)center.Y + 100));
        Thread.Sleep(100);
        FlaUI.Core.Input.Mouse.Up(FlaUI.Core.Input.MouseButton.Left);
        Thread.Sleep(Waits.UISettleMs);

        overlay = _fixture.FindOverlayWindow(WidgetNames.Inputs);
        Assert.NotNull(overlay);
        var movedBounds = overlay.BoundingRectangle;

        Assert.NotEqual(initialBounds.Left, movedBounds.Left);
        Assert.NotEqual(initialBounds.Top, movedBounds.Top);

        // Exit setup mode
        setupButton.Click();
        Thread.Sleep(Waits.UISettleMs);
    }
}
