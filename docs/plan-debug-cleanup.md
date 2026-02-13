# Plan: Debug Code Cleanup

## Current State
- `App.xaml.cs` has `.WriteTo.Console()` in the Serilog configuration (production code)
- All 8 widgets have `UseMockData` flag with hardcoded mock driver names and data — this is intentional for testing and should be kept, but should not be the default
- `Debug.WriteLine` in test code only (acceptable)

## Goal
Ensure production builds don't emit debug output and mock data is clearly opt-in.

## Steps

### 1. Make Console Logging Conditional
- In `App.xaml.cs`, wrap `.WriteTo.Console()` with `#if DEBUG` preprocessor directive
- Production builds should only log to file

### 2. Ensure `UseMockData` Defaults to `false`
- Verify all widget config classes default `UseMockData = false`
- If any default to `true`, change to `false`
- Mock data remains available for development/testing but is never active by default

### 3. Review and Remove Stale Debug Artifacts
- Search for any remaining `Console.WriteLine`, `Debug.WriteLine`, `Debug.Assert` in production code
- Remove or replace with proper `ILogger` calls
- Check for any `TODO`, `HACK`, `FIXME` comments that should be addressed

### Files to Modify
| File | Action |
|------|--------|
| `src/RaceOverlay.App/App.xaml.cs` | Modify — conditional console logging |
| Widget config classes (if any default UseMockData=true) | Modify |

### Acceptance Criteria
- [ ] No `Console.WriteLine` or `.WriteTo.Console()` in Release builds
- [ ] `UseMockData` defaults to `false` in all widget configs
- [ ] No stale debug artifacts in production code paths
