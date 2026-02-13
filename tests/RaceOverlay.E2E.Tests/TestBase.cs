using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace RaceOverlay.E2E.Tests;

/// <summary>
/// Base class for E2E tests that ensures a clean state before each test.
/// Inherit from this class to automatically reset the app state.
/// Note: Tests inheriting from this must still include [Collection("App")] attribute.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly AppFixture Fixture;

    protected TestBase(AppFixture fixture)
    {
        Fixture = fixture;

        // Add a small delay to ensure app is fully initialized
        Thread.Sleep(200);

        ResetAppState();

        // Additional delay after reset to ensure UI automation catches up
        Thread.Sleep(300);
    }

    /// <summary>
    /// Resets the app to a clean state:
    /// - Disables all active widgets
    /// - Exits setup mode if active
    /// - Ensures main window is visible and restored
    /// </summary>
    private void ResetAppState()
    {
        try
        {
            var mainWindow = Fixture.GetMainWindowIncludingHidden();

            // 1. Ensure main window is visible and not minimized
            if (mainWindow.Patterns.Window.IsSupported)
            {
                var state = mainWindow.Patterns.Window.Pattern.WindowVisualState;
                if (state == WindowVisualState.Minimized)
                {
                    mainWindow.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Normal);
                    Thread.Sleep(Waits.UISettleMs);
                }
            }

            // 2. Turn off all widgets
            foreach (var widgetName in WidgetNames.All)
            {
                try
                {
                    var overlay = Fixture.FindOverlayWindow(widgetName);
                    if (overlay != null)
                    {
                        Fixture.ToggleWidgetOff(mainWindow, widgetName);
                    }
                }
                catch
                {
                    // If toggle fails, widget might already be off or in transition
                }
            }

            // 3. Exit setup mode if active
            try
            {
                var setupButton = mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ToggleSetupModeButton"));
                if (setupButton != null)
                {
                    var buttonText = setupButton.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text));
                    // If button says "Exit" or similar, we're in setup mode - click to exit
                    if (buttonText?.Name?.Contains("Exit") == true ||
                        buttonText?.Name?.Contains("Finish") == true)
                    {
                        setupButton.Click();
                        Thread.Sleep(Waits.UISettleMs);
                    }
                }
            }
            catch
            {
                // Setup mode might not be active
            }

            // 4. Give UI time to settle and refresh the UI tree
            Thread.Sleep(Waits.UISettleMs);

            // 5. Refresh the main window to ensure UI tree is up to date
            mainWindow = Fixture.GetMainWindow();
            Thread.Sleep(200); // Extra time for UI automation to catch up
        }
        catch (Exception ex)
        {
            // If reset fails entirely, log but don't fail the test
            System.Diagnostics.Debug.WriteLine($"ResetAppState failed: {ex.Message}");
        }
    }

    public virtual void Dispose()
    {
        // Optional: Clean up after the test as well
        GC.SuppressFinalize(this);
    }
}
