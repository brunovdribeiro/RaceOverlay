# E2E Test Backlog

Comprehensive list of end-to-end tests for RaceOverlay. Each test launches the real app via FlaUI and interacts with it as a user would.

**Legend:** `[x]` implemented, `[-]` skipped (not automatable)

---

## 1. Application Startup & Window — `MainWindowTests.cs`

- [x] `MainWindow_ShouldHaveCorrectTitle` — window title contains "RaceOverlay"
- [x] `MainWindow_ShouldShowWidgetLibrary` — widget library list is present
- [x] `MainWindow_ShouldShowStartLayoutButton` — Start Layout button is present
- [x] `MainWindow_ShouldShowToggleSetupModeButton` — Toggle Setup Mode button is present
- [x] `MainWindow_ShouldOpenCenteredOnScreen` — window starts centered
- [x] `MainWindow_ShouldStartWithNoActiveWidgets` — center panel is empty on clean launch
- [x] `MainWindow_ShouldShowConfigPlaceholder` — right panel shows "Select an active widget to configure"

## 2. Widget Library — `MainWindowTests.cs`

- [x] `WidgetLibrary_ShouldContainExpectedWidgets` — all 8 widget names visible
- [x] `WidgetLibrary_AllTogglesShouldStartOff` — no toggles are checked on clean launch
- [x] `WidgetLibrary_ShouldShowEightItems` — exactly 8 items in the list

## 3. Widget Toggle (per widget) — `WidgetToggleTests.cs`

- [x] `EnableToggle_ShowsOverlayWindow_DisableToggle_HidesIt("Relative Overlay")`
- [x] `EnableToggle_ShowsOverlayWindow_DisableToggle_HidesIt("Fuel Calculator")`
- [x] `EnableToggle_ShowsOverlayWindow_DisableToggle_HidesIt("Inputs")`
- [x] `EnableToggle_ShowsOverlayWindow_DisableToggle_HidesIt("Input Trace")`
- [x] `EnableToggle_ShowsOverlayWindow_DisableToggle_HidesIt("Standings")`
- [x] `EnableToggle_ShowsOverlayWindow_DisableToggle_HidesIt("Lap Timer")`
- [x] `EnableToggle_ShowsOverlayWindow_DisableToggle_HidesIt("Track Map")`
- [x] `EnableToggle_ShowsOverlayWindow_DisableToggle_HidesIt("Weather")`

## 4. Active Widget Cards — `ActiveWidgetCardTests.cs`

- [x] `ToggleWidgetOn_ShouldCreateActiveWidgetCard` — card appears in center panel
- [x] `ToggleWidgetOff_ShouldRemoveActiveWidgetCard` — card removed from center panel
- [x] `ClickActiveWidgetCard_ShouldShowConfigPanel` — right panel switches from placeholder to config
- [x] `ClickDifferentCard_ShouldSwitchConfigPanel` — config panel changes when selecting different card
- [x] `MultipleWidgetsActive_ShouldShowMultipleCards` — enable 3 widgets, see 3 cards

## 5. Configuration Panel — `ConfigurationPanelTests.cs`

### General
- [x] `NoWidgetSelected_ShouldShowPlaceholder` — "Select an active widget to configure" visible
- [x] `SelectWidget_ShouldShowWidgetSpecificSettings` — correct section visible based on widget type
- [x] `SelectDifferentWidget_ShouldSwitchConfigPanel` — panel changes when selecting different card

### Relative Overlay Config
- [x] `RelativeOverlay_ToggleCheckbox_ShouldBeInteractable("Position")`
- [x] `RelativeOverlay_ToggleCheckbox_ShouldBeInteractable("Class Color")`
- [x] `RelativeOverlay_ToggleCheckbox_ShouldBeInteractable("Driver Name")`
- [x] `RelativeOverlay_TextBox_ShouldBeEditable("Drivers Ahead")`
- [x] `RelativeOverlay_TextBox_ShouldBeEditable("Drivers Behind")`
- [x] `RelativeOverlay_TextBox_ShouldBeEditable("Update Interval (ms)")`

### Fuel Calculator Config
- [x] `FuelCalculator_ChangeTankCapacity_ShouldUpdateConfig`

### Inputs Config
- [x] `Inputs_ChangeUpdateInterval_ShouldUpdateConfig`
- [x] `Inputs_ChangeColor_ShouldUpdateConfig("Throttle")`
- [x] `Inputs_ChangeColor_ShouldUpdateConfig("Brake")`
- [x] `Inputs_ToggleShowClutch_ShouldUpdateConfig`

### Standings Config
- [x] `Standings_ChangeMaxDrivers_ShouldUpdateConfig`
- [x] `Standings_ToggleColumn_ShouldBeInteractable` — 10 column toggles via Theory

### Lap Timer Config
- [x] `LapTimer_ToggleCheckbox_ShouldBeInteractable("Show Delta to Best")`
- [x] `LapTimer_ToggleCheckbox_ShouldBeInteractable("Show Last Lap")`
- [x] `LapTimer_ToggleCheckbox_ShouldBeInteractable("Show Best Lap")`

### Track Map Config
- [x] `TrackMap_ToggleCheckbox_ShouldBeInteractable("Show Driver Names")`
- [x] `TrackMap_ToggleCheckbox_ShouldBeInteractable("Show Pit Status")`

### Weather Config
- [x] `Weather_ToggleCheckbox_ShouldBeInteractable("Show Wind")`
- [x] `Weather_ToggleCheckbox_ShouldBeInteractable("Show Forecast")`

### Input Trace Config
- [x] `InputTrace_ChangeHistorySeconds_ShouldUpdateConfig`
- [x] `InputTrace_ChangeThrottleColor_ShouldUpdateConfig`

## 6. Setup / Drag Mode — `SetupModeTests.cs`

- [x] `ToggleSetupMode_ButtonTextChanges` — button text updates on toggle
- [x] `SetupMode_OverlayWindowShowsDragIndicator` — "Drag Mode" text visible on overlay
- [x] `DragMode_CanMoveOverlayWindow` — window position changes after drag

## 7. Start Layout — `StartLayoutTests.cs`

- [x] `StartLayout_MinimizesMainWindow` — main window hides after clicking Start Layout
- [x] `StartLayout_OverlayWindowsRemainVisible` — overlay windows still on screen

## 8. System Tray — `SystemTrayTests.cs`

- [x] `CloseMainWindow_ShouldMinimizeToTray` — X button hides window, doesn't exit
- [-] `TrayIcon_DoubleClick_RestoresMainWindow` — Win32 NotifyIcon, not automatable via UIA
- [-] `TrayMenu_Exit_ShutsDownApp` — Win32 ContextMenuStrip, not automatable via UIA

## 9. Error Dialog — `ErrorDialogTests.cs`

- [-] `ErrorDialog_ShouldShowOnUnhandledException` — requires app test-mode hook to trigger
- [-] `ErrorDialog_CopyButton_CopiesToClipboard` — requires app test-mode hook to trigger
- [-] `ErrorDialog_CloseButton_DismissesDialog` — requires app test-mode hook to trigger
- [-] `ErrorDialog_ShouldDisplayExceptionDetails` — requires app test-mode hook to trigger

## 10. Config Persistence — `ConfigPersistenceTests.cs`

- [x] `EnableWidget_CloseApp_ReopenApp_WidgetRestored` — widget is active after restart
- [x] `NoConfigFile_StartsClean` — first launch has no active widgets
- [x] `CorruptConfigFile_StartsClean` — invalid JSON doesn't crash app

---

## Summary

| Category | Implemented | Skipped | Total |
|----------|:-----------:|:-------:|:-----:|
| Startup & Window | 7 | 0 | 7 |
| Widget Library | 3 | 0 | 3 |
| Widget Toggle | 8 | 0 | 8 |
| Active Widget Cards | 5 | 0 | 5 |
| Configuration Panel | 28 | 0 | 28 |
| Setup / Drag Mode | 3 | 0 | 3 |
| Start Layout | 2 | 0 | 2 |
| System Tray | 1 | 2 | 3 |
| Error Dialog | 0 | 4 | 4 |
| Config Persistence | 3 | 0 | 3 |
| **Total** | **60** | **6** | **66** |
