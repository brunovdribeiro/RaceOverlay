# Beta Readiness Assessment

> Last updated: 2026-02-13

## Critical Blockers (must fix)

### 1. ~~No Configuration Persistence~~ — DONE
`ConfigurationPersistenceService` handles JSON-based save/load of widget positions, settings, and layouts.

### 2. ~~Game Providers Are Empty Scaffolds~~ — DONE
iRacing provider is fully implemented using `IRSDKSharper`, reading real telemetry (speed, RPM, gear, inputs, lap times, driver/session info, track data).

### 3. ~~No Global Exception Handler~~ — DONE
`DispatcherUnhandledException` is wired up in `App.xaml.cs`.

### 4. ~~No Versioning~~ / No Installer — PARTIALLY DONE
Versioning is centralized in `Directory.Build.props` (0.1.0-beta.1) with CI tag-based overrides. However, no MSIX/WiX installer or publish profile exists. See [plan-installer.md](plan-installer.md).

---

## High Priority (should fix)

### 5. ~~No File-Based Logging~~ — DONE
`ILogger` integration is in place.

### 6. ~~No CI/CD Pipeline~~ — DONE
GitHub Actions workflow exists at `.github/workflows/ci.yml`.

### 7. ~~0% Test Coverage~~ — DONE
43 tests across 11 files (E2E and unit tests).

### 8. No Input Validation
Settings textboxes likely still accept negative numbers, non-numeric values, etc.

---

## Nice to Have for Beta

### 9. UI Polish
No about dialog, no version display, no loading indicators.

### 10. Architecture Debt
- `WidgetOverlayWindow` may still have a large if/else chain for widget types.
- Config loading may be duplicated in `MainWindowViewModel`.

### 11. Debug Code Cleanup
`Debug.WriteLine` calls, `AddConsole()` logging still in production path, hardcoded mock driver names.

---

## What's Already Solid

- Widget architecture and MVVM patterns are clean
- All 8 MVP widgets implemented (Relative, Fuel Calculator, Inputs, Input Trace, Standings, Lap Timer, Track Map, Weather)
- Dark theme UI looks professional
- Drag-to-reposition with CTRL+F12 works
- Event subscription cleanup prevents memory leaks
- Cancellation token usage is correct
- Configuration persistence via JSON
- Global exception handling
- CI pipeline with GitHub Actions
- E2E and unit test coverage
- README and internal docs are excellent
- NuGet dependency management via Directory.Packages.props

---

## Remaining Priority Order

| # | Task | Effort | Status |
|---|------|--------|--------|
| 1 | ~~Implement at least one real game provider (iRacing)~~ | Large | **Done** |
| 2 | ~~Add assembly versioning~~ | Small | **Done** (Directory.Build.props) |
| 3 | Create a basic installer (MSIX) | Medium | Not started — [plan](plan-installer.md) |
| 4 | Add input validation on settings | Small | Not started — [plan](plan-input-validation.md) |
| 5 | UI polish (about dialog, connection status) | Small | Not started — [plan](plan-ui-polish.md) |
| 6 | Architecture debt cleanup | Medium | Not started — [plan](plan-architecture-debt.md) |
| 7 | Debug code cleanup | Small | Not started — [plan](plan-debug-cleanup.md) |

Item 3 (installer) is the main remaining blocker for distribution. Items 4-7 improve quality but are not strictly required for beta.
