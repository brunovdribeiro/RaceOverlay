namespace RaceOverlay.E2E.Tests;

[Collection("App")]
public class ConfigurationPanelTests
{
    private readonly AppFixture _fixture;

    public ConfigurationPanelTests(AppFixture fixture) => _fixture = fixture;

    // --- Shared assertion helpers ---

    private void AssertCheckboxToggleable(string widgetName, string checkboxContent)
    {
        using var _ = _fixture.ActivateAndSelect(widgetName);
        var mainWindow = _fixture.GetMainWindow();

        var checkbox = _fixture.FindCheckBox(mainWindow, checkboxContent);
        Assert.NotNull(checkbox);

        var initial = checkbox.IsChecked;
        checkbox.Click();
        Thread.Sleep(Waits.ConfigChangeMs);
        Assert.NotEqual(initial, checkbox.IsChecked);

        checkbox.Click(); // Restore
    }

    private void AssertTextBoxEditable(string widgetName, string label, string testValue)
    {
        using var _ = _fixture.ActivateAndSelect(widgetName);
        var mainWindow = _fixture.GetMainWindow();

        var textBox = _fixture.FindTextBoxByLabel(mainWindow, label);
        Assert.NotNull(textBox);

        var original = textBox.Text;
        textBox.Text = testValue;
        Thread.Sleep(Waits.ConfigChangeMs);
        Assert.Equal(testValue, textBox.Text);

        textBox.Text = original; // Restore
    }

    // --- General ---

    [Fact]
    public void NoWidgetSelected_ShouldShowPlaceholder()
    {
        Assert.NotNull(_fixture.GetMainWindow()
            .FindFirstDescendant(cf => cf.ByText("Select an active widget to configure")));
    }

    [Fact]
    public void SelectWidget_ShouldShowWidgetSpecificSettings()
    {
        using var _ = _fixture.ActivateAndSelect(WidgetNames.RelativeOverlay);

        Assert.NotNull(_fixture.GetMainWindow()
            .FindFirstDescendant(cf => cf.ByText("COLUMNS")));
    }

    [Fact]
    public void SelectDifferentWidget_ShouldSwitchConfigPanel()
    {
        using var _ = _fixture.ActivateWidgets(WidgetNames.RelativeOverlay, WidgetNames.LapTimer);
        var mainWindow = _fixture.GetMainWindow();

        _fixture.ClickActiveWidgetCard(mainWindow, WidgetNames.RelativeOverlay);
        Assert.NotNull(mainWindow.FindFirstDescendant(cf => cf.ByText("COLUMNS")));

        _fixture.ClickActiveWidgetCard(mainWindow, WidgetNames.LapTimer);
        Assert.NotNull(mainWindow.FindFirstDescendant(cf => cf.ByText("LAP TIMER SETTINGS")));
    }

    // --- Relative Overlay Config ---

    [Theory]
    [InlineData("Position")]
    [InlineData("Class Color")]
    [InlineData("Driver Name")]
    public void RelativeOverlay_ToggleCheckbox_ShouldBeInteractable(string checkboxContent) =>
        AssertCheckboxToggleable(WidgetNames.RelativeOverlay, checkboxContent);

    [Theory]
    [InlineData("Drivers Ahead")]
    [InlineData("Drivers Behind")]
    [InlineData("Update Interval (ms)")]
    public void RelativeOverlay_TextBox_ShouldBeEditable(string label) =>
        AssertTextBoxEditable(WidgetNames.RelativeOverlay, label, "99");

    // --- Fuel Calculator Config ---

    [Fact]
    public void FuelCalculator_ChangeTankCapacity_ShouldUpdateConfig() =>
        AssertTextBoxEditable(WidgetNames.FuelCalculator, "Tank Capacity (L)", "85");

    // --- Inputs Config ---

    [Fact]
    public void Inputs_ChangeUpdateInterval_ShouldUpdateConfig() =>
        AssertTextBoxEditable(WidgetNames.Inputs, "Update Interval (ms)", "50");

    [Theory]
    [InlineData("Throttle")]
    [InlineData("Brake")]
    public void Inputs_ChangeColor_ShouldUpdateConfig(string label) =>
        AssertTextBoxEditable(WidgetNames.Inputs, label, "#FF0000");

    [Fact]
    public void Inputs_ToggleShowClutch_ShouldUpdateConfig() =>
        AssertCheckboxToggleable(WidgetNames.Inputs, "Show Clutch");

    // --- Standings Config ---

    [Fact]
    public void Standings_ChangeMaxDrivers_ShouldUpdateConfig() =>
        AssertTextBoxEditable(WidgetNames.Standings, "Max Drivers", "10");

    [Theory]
    [InlineData("Class Color")]
    [InlineData("Car Number")]
    [InlineData("Positions Gained")]
    [InlineData("License")]
    [InlineData("iRating")]
    [InlineData("Car Brand")]
    [InlineData("Interval")]
    [InlineData("Last Lap Time")]
    [InlineData("Delta")]
    [InlineData("Pit Status")]
    public void Standings_ToggleColumn_ShouldBeInteractable(string columnName) =>
        AssertCheckboxToggleable(WidgetNames.Standings, columnName);

    // --- Lap Timer Config ---

    [Theory]
    [InlineData("Show Delta to Best")]
    [InlineData("Show Last Lap")]
    [InlineData("Show Best Lap")]
    public void LapTimer_ToggleCheckbox_ShouldBeInteractable(string checkboxContent) =>
        AssertCheckboxToggleable(WidgetNames.LapTimer, checkboxContent);

    // --- Track Map Config ---

    [Theory]
    [InlineData("Show Driver Names")]
    [InlineData("Show Pit Status")]
    public void TrackMap_ToggleCheckbox_ShouldBeInteractable(string checkboxContent) =>
        AssertCheckboxToggleable(WidgetNames.TrackMap, checkboxContent);

    // --- Weather Config ---

    [Theory]
    [InlineData("Show Wind")]
    [InlineData("Show Forecast")]
    public void Weather_ToggleCheckbox_ShouldBeInteractable(string checkboxContent) =>
        AssertCheckboxToggleable(WidgetNames.Weather, checkboxContent);

    // --- Input Trace Config ---

    [Fact]
    public void InputTrace_ChangeHistorySeconds_ShouldUpdateConfig() =>
        AssertTextBoxEditable(WidgetNames.InputTrace, "History (seconds)", "15");

    [Fact]
    public void InputTrace_ChangeThrottleColor_ShouldUpdateConfig() =>
        AssertTextBoxEditable(WidgetNames.InputTrace, "Throttle", "#00FF00");
}
