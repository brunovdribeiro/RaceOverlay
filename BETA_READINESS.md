# Beta Readiness Assessment

## Critical Blockers (must fix)

### 1. No Configuration Persistence
Widget positions, settings, and layouts are only stored in memory. Everything resets on app restart. Need JSON file-based save/load.

### 2. Game Providers Are Empty Scaffolds
iRacing, Assetto Corsa, F1 24 providers have no implementation. All widget data is mock/random. This is the core value proposition of the app.

### 3. No Global Exception Handler
No `DispatcherUnhandledException` in App.xaml.cs. Any unhandled error will crash the app with a generic Windows dialog.

### 4. No Installer or Versioning
No MSIX/WiX installer, no assembly version info, no publish profile. Users can't install or update the app.

---

## High Priority (should fix)

### 5. No File-Based Logging
Only console/debug output. Users can't send log files to diagnose issues.

### 6. No CI/CD Pipeline
No GitHub Actions, no automated builds or tests on push.

### 7. 0% Test Coverage
Test projects exist but contain only empty placeholder tests.

### 8. No Input Validation
Settings textboxes accept negative numbers, non-numeric values, etc.

---

## Nice to Have for Beta

### 9. UI Polish
No about dialog, no system tray icon, no version display, no loading indicators.

### 10. Architecture Debt
- `WidgetOverlayWindow` has a massive if/else chain for widget types that will break with each new widget.
- Config loading is heavily duplicated in `MainWindowViewModel`.

### 11. Debug Code Cleanup
`Debug.WriteLine` calls, `AddConsole()` logging still in production path, hardcoded mock driver names.

---

## What's Already Solid

- Widget architecture and MVVM patterns are clean
- Dark theme UI looks professional
- Drag-to-reposition with CTRL+F12 works
- Event subscription cleanup prevents memory leaks
- Cancellation token usage is correct
- README and internal docs are excellent
- NuGet dependency management via Directory.Packages.props

---

## Recommended Priority Order

| # | Task | Effort |
|---|------|--------|
| 1 | Implement at least one real game provider | Large |
| 2 | Add JSON config persistence (save/load settings) | Medium |
| 3 | Add global exception handler + error dialog | Small |
| 4 | Add assembly versioning | Small |
| 5 | Add file logging (Serilog or similar) | Small |
| 6 | Create a basic installer (MSIX) | Medium |
| 7 | Add input validation on settings | Small |
| 8 | Set up GitHub Actions CI | Medium |

Items 1-4 are the minimum for a usable beta. Without a real game provider, the app doesn't do anything meaningful. Without config persistence, users will have to reconfigure every launch.
