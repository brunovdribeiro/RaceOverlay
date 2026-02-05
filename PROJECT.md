# RaceOverlay - Project Overview

## What Is This?

RaceOverlay is a Windows desktop application that reads live telemetry from sim racing games and renders transparent overlay widgets on top of the game. The primary target is **iRacing**, with planned support for Assetto Corsa and F1 24. The direct competitor is **RaceLabs**.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 |
| UI Framework | WPF (Windows Presentation Foundation) |
| MVVM Toolkit | CommunityToolkit.Mvvm 8.3.2 |
| DI Container | Microsoft.Extensions.DependencyInjection 9.0.0 |
| Hosting | Microsoft.Extensions.Hosting 9.0.0 |
| Serialization | System.Text.Json 9.0.0 |
| Testing | xUnit 2.9.2, Moq 4.20.72, FluentAssertions 6.12.1 |
| Language | C# 12, nullable reference types enforced |

## Solution Structure

```
RaceOverlay.sln
│
├── src/
│   ├── RaceOverlay.Core/              # Pure .NET library (no WPF dependency)
│   │   ├── IGameProvider.cs           # Contract for telemetry providers
│   │   ├── TelemetryData.cs           # Immutable telemetry snapshot
│   │   └── Widgets/
│   │       ├── IWidget.cs             # Contract for overlay widgets
│   │       ├── IWidgetConfiguration.cs
│   │       ├── IRelativeOverlayConfig.cs
│   │       └── WidgetMetadata.cs
│   │
│   ├── RaceOverlay.Engine/            # WPF library — services, views, viewmodels
│   │   ├── Widgets/
│   │   │   ├── IWidgetRegistry.cs
│   │   │   ├── WidgetRegistry.cs      # Discovers and instantiates widgets
│   │   │   └── RelativeOverlay.cs     # First real widget implementation
│   │   ├── Models/
│   │   │   └── RelativeDriver.cs      # Driver data for relative board
│   │   ├── ViewModels/
│   │   │   └── RelativeOverlayViewModel.cs
│   │   └── Views/
│   │       └── RelativeOverlayView.xaml
│   │
│   ├── RaceOverlay.App/               # WPF executable — shell, DI, hotkeys
│   │   ├── App.xaml(.cs)              # HostBuilder + DI registration
│   │   ├── MainWindow.xaml(.cs)       # Main control panel
│   │   ├── WidgetOverlayWindow.xaml   # Transparent overlay host
│   │   ├── Services/
│   │   │   └── WidgetDragService.cs   # Global drag mode toggle
│   │   ├── Converters/
│   │   └── Themes/
│   │       └── Theme.xaml             # Dark color palette
│   │
│   └── Providers/                     # One project per game (scaffolded)
│       ├── RaceOverlay.Providers.iRacing/
│       ├── RaceOverlay.Providers.AssettoCorsa/
│       └── RaceOverlay.Providers.F124/
│
└── tests/
    ├── RaceOverlay.Core.Tests/
    ├── RaceOverlay.Engine.Tests/
    └── RaceOverlay.Providers.Tests/
```

## Architecture

```
┌──────────────────────────────────────────────┐
│                 RaceOverlay.App               │
│  MainWindow  WidgetOverlayWindow  DragService│
└──────────────────┬───────────────────────────┘
                   │ references
┌──────────────────▼───────────────────────────┐
│               RaceOverlay.Engine              │
│  WidgetRegistry  ViewModels  Views  Models   │
└──────────────────┬───────────────────────────┘
                   │ references
┌──────────────────▼───────────────────────────┐
│                RaceOverlay.Core               │
│  IGameProvider  IWidget  TelemetryData       │
└──────────────────▲───────────────────────────┘
                   │ references
┌──────────────────┴───────────────────────────┐
│  Providers.iRacing / AssettoCorsa / F124     │
│  (implement IGameProvider per game)          │
└──────────────────────────────────────────────┘
```

**Key patterns:** MVVM, Dependency Injection, Plugin/Registry, Observer (events for telemetry).

## Core Contracts

### IGameProvider — telemetry source

```csharp
public interface IGameProvider
{
    string GameId { get; }
    string DisplayName { get; }
    bool IsGameRunning();
    Task StartAsync(CancellationToken ct);
    Task StopAsync();
    event EventHandler<TelemetryData>? DataReceived;
}
```

### TelemetryData — single telemetry frame

Speed, RPM, Gear, Throttle, Brake, Clutch, CurrentLapTime, LastLapTime, BestLapTime, LapNumber, TrackName, CarName, Timestamp. All init-only (immutable).

### IWidget — overlay component

```csharp
public interface IWidget
{
    string WidgetId { get; }
    string DisplayName { get; }
    string Description { get; }
    IWidgetConfiguration Configuration { get; }
    void UpdateConfiguration(IWidgetConfiguration config);
    Task StartAsync(CancellationToken ct);
    Task StopAsync();
}
```

### IWidgetRegistry — widget discovery and creation

Stores `WidgetMetadata` entries. Creates widget instances via `ActivatorUtilities.CreateInstance()` so widgets receive injected dependencies.

## What Works Today

| Feature | Status |
|---------|--------|
| MVVM infrastructure + DI | Done |
| Widget registry & lifecycle | Done |
| Relative Overlay widget (mock data) | Done |
| Overlay windows (transparent, topmost) | Done |
| Widget drag/reposition (CTRL+F12 toggle) | Done |
| Dark theme with color palette | Done |
| Test project scaffolding | Done |
| iRacing telemetry provider | Not started |
| Assetto Corsa / F1 24 providers | Not started |
| Configuration persistence (JSON) | Not started |
| Widget position saving | Not started |

## Existing Widget: Relative Overlay

The only implemented widget. Shows drivers around the player sorted by track position.

**Data per driver (RelativeDriver model):**
Position, Number, DriverName, VehicleClass, ClassColor, EloRating, EloGrade, CurrentLapTime, BestLapTime, DeltaFromBest, GapToNextDriver, StintLapsCompleted/Total, StintTime, IsInPit, StatusFlag, HasDamage, TrackDistanceMeters, RelativePosition.

**Current behavior:** Generates mock data simulating 7 drivers (3 ahead, player, 3 behind) with randomized telemetry. Updates every 500 ms.

## Theme

Dark palette defined in `Themes/Theme.xaml`. Resource keys prefixed `RO.`:

| Key | Color | Usage |
|-----|-------|-------|
| `RO.BackgroundBrush` | #0B0F14 | Window backgrounds |
| `RO.SurfaceBrush` | #161B22 | Cards, panels |
| `RO.ForegroundBrush` | #E7EAF0 | Primary text |
| `RO.MutedBrush` | #9AA3B2 | Secondary text |
| `RO.PurpleBrush` | #6D28D9 | Primary accent |
| `RO.MagentaBrush` | #D946EF | Secondary accent |
| `RO.GreenBrush` | #10B981 | Positive / active |
| `RO.RedBrush` | #EF4444 | Negative / warnings |
| `RO.BorderBrush` | #1E293B | Borders |

## Hotkeys

| Shortcut | Action |
|----------|--------|
| CTRL+F12 | Toggle widget drag mode on/off |

## Build & Run

```
dotnet build RaceOverlay.sln
dotnet run --project src/RaceOverlay.App
dotnet test
```

Requires .NET 10 SDK. Windows only (WPF).

---

## Feature Roadmap

### P0 — Connect to iRacing

- [ ] Implement `IGameProvider` for iRacing using shared memory / telemetry SDK
- [ ] Wire `TelemetryData` events into the Relative Overlay widget (replace mock data)
- [ ] Auto-detect when iRacing is running
- [ ] Session info parsing (track, car, session type, weather)

### P1 — Essential Widgets (RaceLabs parity)

- [ ] **Standings Widget** — full race standings with class filtering
- [ ] **Timing Widget** — current lap, last lap, best lap, sector splits, delta bar
- [ ] **Inputs Widget** — steering wheel angle, throttle/brake/clutch bars
- [ ] **Track Map Widget** — 2D track outline with car dots, colored by class
- [ ] **Fuel Calculator Widget** — fuel remaining, consumption per lap, laps of fuel left, fuel to add at pit
- [ ] **Pit Strategy Widget** — tire wear, optimal pit window, mandatory pit tracking
- [ ] **Weather Widget** — current/forecast conditions, track temp, air temp

### P2 — Competitive Advantage

- [ ] **Radar/Proximity Widget** — top-down spotter showing nearby cars with distance
- [ ] **Race Engineer Widget** — AI-driven pit strategy suggestions, tire degradation predictions
- [ ] **Telemetry Graphs Widget** — real-time line charts for speed, throttle, brake traces
- [ ] **Driver Comparison Widget** — side-by-side sector-level comparison with a target driver
- [ ] **Incident Tracker Widget** — track incident count, penalties, protest-worthy contacts
- [ ] **Spotter Audio** — configurable audio callouts (clear left, car right, 3-wide)

### P3 — Platform & Polish

- [ ] Configuration persistence to `%APPDATA%\RaceOverlay\` (JSON)
- [ ] Widget position and size saving across sessions
- [ ] Per-game / per-car / per-track profiles
- [ ] Preset import/export and sharing
- [ ] Settings UI (configuration panel per widget)
- [ ] Auto-update mechanism
- [ ] Assetto Corsa provider (UDP telemetry, port 11111)
- [ ] F1 24 provider (UDP telemetry, port 20777)
- [ ] Plugin system for third-party widget distribution

### P4 — Future Possibilities

- [ ] Web dashboard / companion app (remote telemetry viewer)
- [ ] Race replay analysis with telemetry overlay
- [ ] Multi-monitor / VR overlay support
- [ ] Community widget marketplace
- [ ] Cloud sync for settings and profiles
- [ ] Discord/Twitch integration for streamers

---

## RaceLabs Comparison

Key features RaceLabs offers that we should target:

| RaceLabs Feature | Our Status | Priority |
|-----------------|------------|----------|
| Relative board | Mock data only | P0 |
| Live standings | Not started | P1 |
| Fuel calculator | Not started | P1 |
| Track map | Not started | P1 |
| Input display | Not started | P1 |
| Timing/delta bar | Not started | P1 |
| Spotter/radar | Not started | P2 |
| Pit strategy | Not started | P1 |
| Widget customization UI | Not started | P3 |
| Multi-sim support | Scaffolded | P3 |
| Preset sharing | Not started | P3 |

## Conventions

- **Namespaces** mirror folder structure: `RaceOverlay.Core.Widgets`, `RaceOverlay.Engine.Models`
- **DI registration** in `App.xaml.cs` via `HostBuilder`
- **Theme colors** referenced as `{StaticResource RO.<Name>Brush}`
- **ViewModels** use `[ObservableProperty]` and `[RelayCommand]` source generators
- **Nullable** reference types enforced project-wide (`WarningsAsErrors: nullable`)
