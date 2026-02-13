# Plan: UI Polish

## Current State
- Title bar shows version (via `AssemblyInformationalVersionAttribute`)
- System tray icon works with context menu (Open / Exit) and minimize-to-tray
- No About dialog, no help/documentation links, no loading indicators

## Goal
Add the small UI touches that make the app feel complete for beta users.

## Steps

### 1. About Dialog
- Create `AboutWindow.xaml` in `src/RaceOverlay.App/`
- Display: app name, version, build date, copyright, GitHub link
- Read version from assembly attributes (already done in title bar)
- Add "About" menu item to system tray context menu
- Style to match existing dark theme

### 2. Loading / Connection Status Indicator
- Add a status bar or indicator in `MainWindow.xaml` showing:
  - "Connected to iRacing" / "Waiting for iRacing..." / "Disconnected"
  - Small colored dot (green/yellow/red)
- Subscribe to `IRacingDataService.OnConnected` / `OnDisconnected` events
- Show in bottom of the main window

### 3. Help / Links
- Add "Documentation" link in tray context menu or main window that opens the GitHub repo README
- Add keyboard shortcut reference tooltip (CTRL+F12 for setup mode)

### Files to Create/Modify
| File | Action |
|------|--------|
| `src/RaceOverlay.App/AboutWindow.xaml` | Create |
| `src/RaceOverlay.App/AboutWindow.xaml.cs` | Create |
| `src/RaceOverlay.App/MainWindow.xaml` | Modify — add connection status indicator |
| `src/RaceOverlay.App/MainWindow.xaml.cs` | Modify — add About menu to tray, status binding |
| `src/RaceOverlay.App/ViewModels/MainWindowViewModel.cs` | Modify — add connection status property |

### Acceptance Criteria
- [ ] About dialog shows version, build info, and GitHub link
- [ ] Tray context menu has "About" entry
- [ ] Main window shows live connection status to game provider
- [ ] Dark theme is consistent across new UI elements
