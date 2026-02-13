using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace RaceOverlay.E2E.Tests;

/// <summary>
/// Shared fixture that launches the app once and tears it down after all tests complete.
/// Backs up and removes the user's config so the app starts in a clean state.
/// </summary>
public class AppFixture : IDisposable
{
    private const int OffscreenThresholdPx = 5000;

    public static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RaceOverlay");

    public static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");
    private static readonly string ConfigBackupPath = ConfigPath + ".e2e-backup";

    public Application App { get; }
    public UIA3Automation Automation { get; }

    public AppFixture()
    {
        BackupConfig();

        var exePath = FindAppExecutable();
        App = Application.Launch(exePath);
        Automation = new UIA3Automation();

        var mainWindow = App.GetMainWindow(Automation, Waits.StartupTimeout);
        if (mainWindow == null)
            throw new InvalidOperationException("Main window did not appear within the timeout period.");
    }

    // --- Window helpers ---

    public Window GetMainWindow()
    {
        return App.GetMainWindow(Automation, TimeSpan.FromSeconds(5))
            ?? throw new InvalidOperationException("Main window not found.");
    }

    public Window? FindOverlayWindow(string title)
    {
        var allWindows = App.GetAllTopLevelWindows(Automation);
        return allWindows.FirstOrDefault(w => w.Title == title);
    }

    public bool IsWindowMinimized(Window window)
    {
        var bounds = window.BoundingRectangle;
        return bounds.Width == 0 || bounds.Height == 0 || bounds.Top > OffscreenThresholdPx;
    }

    // --- Widget toggle helpers ---

    public AutomationElement? FindWidgetToggle(Window mainWindow, string widgetName)
    {
        var label = mainWindow.FindFirstDescendant(cf => cf.ByText(widgetName));
        if (label?.Parent == null) return null;
        return label.Parent.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));
    }

    public void ToggleWidgetOn(Window mainWindow, string widgetName)
    {
        var toggle = FindWidgetToggle(mainWindow, widgetName)
            ?? throw new InvalidOperationException($"Toggle not found for '{widgetName}'");
        InvokeToggle(toggle);
        Retry.WhileFalse(
            () => FindOverlayWindow(widgetName) != null,
            timeout: Waits.OverlayTimeout,
            interval: Waits.RetryInterval);
    }

    public void ToggleWidgetOff(Window mainWindow, string widgetName)
    {
        var toggle = FindWidgetToggle(mainWindow, widgetName)
            ?? throw new InvalidOperationException($"Toggle not found for '{widgetName}'");
        InvokeToggle(toggle);
        Retry.WhileTrue(
            () => FindOverlayWindow(widgetName) != null,
            timeout: Waits.OverlayTimeout,
            interval: Waits.RetryInterval);
    }

    /// <summary>
    /// Invokes a toggle via UIA patterns instead of mouse click,
    /// so Topmost overlay windows don't intercept the interaction.
    /// </summary>
    private static void InvokeToggle(AutomationElement toggle)
    {
        if (toggle.Patterns.Toggle.IsSupported)
            toggle.Patterns.Toggle.Pattern.Toggle();
        else if (toggle.Patterns.Invoke.IsSupported)
            toggle.Patterns.Invoke.Pattern.Invoke();
        else
            toggle.Click();
    }

    /// <summary>
    /// Returns a disposable scope that activates widget(s) on creation
    /// and deactivates them on dispose. Replaces try/finally boilerplate.
    /// </summary>
    public WidgetScope ActivateWidgets(params string[] widgetNames)
    {
        return new WidgetScope(this, widgetNames);
    }

    // --- Card & config panel helpers ---

    public AutomationElement? FindCardInCenterPanel(Window mainWindow, string widgetName)
    {
        var widgetId = widgetName.ToLower().Replace(" ", "-");
        return mainWindow.FindFirstDescendant(cf => cf.ByAutomationId(widgetId));
    }

    public void ClickActiveWidgetCard(Window mainWindow, string widgetName)
    {
        var card = FindCardInCenterPanel(mainWindow, widgetName);
        card?.Click();
        Thread.Sleep(Waits.UISettleMs);
    }

    /// <summary>
    /// Activates a widget, clicks its card, and returns — ready for config assertions.
    /// Returns a WidgetScope that deactivates on dispose.
    /// </summary>
    public WidgetScope ActivateAndSelect(string widgetName)
    {
        var scope = ActivateWidgets(widgetName);
        ClickActiveWidgetCard(GetMainWindow(), widgetName);
        return scope;
    }

    // --- Element finders ---

    public CheckBox? FindCheckBox(Window mainWindow, string content)
    {
        var checkboxes = mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.CheckBox));
        var match = checkboxes.FirstOrDefault(cb => cb.Name == content && !cb.IsOffscreen);
        return match?.AsCheckBox();
    }

    public TextBox? FindTextBoxByLabel(Window mainWindow, string labelText)
    {
        var labels = mainWindow.FindAllDescendants(cf => cf.ByText(labelText));
        foreach (var label in labels)
        {
            if (label.IsOffscreen) continue;
            var parent = label.Parent;
            if (parent == null) continue;
            var textBox = parent.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit));
            if (textBox != null)
                return textBox.AsTextBox();
        }
        return null;
    }

    // --- Config file management (static, usable from any context) ---

    public static void BackupConfig()
    {
        if (File.Exists(ConfigPath))
        {
            File.Copy(ConfigPath, ConfigBackupPath, overwrite: true);
            File.Delete(ConfigPath);
        }
    }

    public static void RestoreConfig()
    {
        if (File.Exists(ConfigBackupPath))
        {
            File.Copy(ConfigBackupPath, ConfigPath, overwrite: true);
            File.Delete(ConfigBackupPath);
        }
    }

    public static void DeleteConfig()
    {
        if (File.Exists(ConfigPath))
            File.Delete(ConfigPath);
    }

    public static string FindAppExecutable()
    {
        var baseDir = AppContext.BaseDirectory;
        var repoRoot = FindRepoRoot(baseDir);

        var configurations = new[] { "Debug", "Release" };
        foreach (var config in configurations)
        {
            var exePath = Path.Combine(repoRoot, "src", "RaceOverlay.App", "bin", config,
                "net10.0-windows", "RaceOverlay.App.exe");
            if (File.Exists(exePath))
                return exePath;
        }

        throw new FileNotFoundException(
            "Could not find RaceOverlay.App.exe. " +
            "Build the app first: dotnet build src/RaceOverlay.App/RaceOverlay.App.csproj");
    }

    public void Dispose()
    {
        App?.Kill();
        Automation?.Dispose();
        RestoreConfig();
        GC.SuppressFinalize(this);
    }

    private static string FindRepoRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not find repository root (.git directory).");
    }
}

/// <summary>
/// Disposable scope that deactivates widgets when done. Use via AppFixture.ActivateWidgets().
/// </summary>
public sealed class WidgetScope : IDisposable
{
    private readonly AppFixture _fixture;
    private readonly string[] _widgetNames;

    public WidgetScope(AppFixture fixture, string[] widgetNames)
    {
        _fixture = fixture;
        _widgetNames = widgetNames;

        var mainWindow = fixture.GetMainWindow();
        foreach (var name in widgetNames)
            fixture.ToggleWidgetOn(mainWindow, name);
    }

    public void Dispose()
    {
        var mainWindow = _fixture.GetMainWindow();
        // Deactivate in reverse order
        for (int i = _widgetNames.Length - 1; i >= 0; i--)
            _fixture.ToggleWidgetOff(mainWindow, _widgetNames[i]);
    }
}

[CollectionDefinition("App")]
public class AppCollection : ICollectionFixture<AppFixture> { }
