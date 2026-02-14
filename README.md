# RaceOverlay

**A powerful, customizable overlay system for sim racing games**

RaceOverlay is a Windows desktop application that displays real-time telemetry data and customizable widgets on top of racing simulators like iRacing, Assetto Corsa, and F1 24. Think of it as a modular, developer-friendly alternative to RaceLabs with a plugin-based architecture.

---

## 🎯 Purpose

RaceOverlay aims to provide sim racing enthusiasts with:
- **Real-time race data visualization** through transparent overlay widgets
- **Customizable layouts** with drag-and-drop widget positioning
- **Multi-game support** via pluggable game providers
- **Extensible architecture** for community-developed widgets
- **Performance-focused design** with minimal FPS impact

---

## 🚀 Features

### Current Status
- ✅ MVVM infrastructure with Dependency Injection
- ✅ Widget registry and lifecycle management
- ✅ Transparent, click-through overlay windows
- ✅ Widget drag/reposition mode (CTRL+F12)
- ✅ Dark theme with custom color palette
- ✅ Multiple functional widgets with mock data
- ⏳ Game provider implementations (in progress)

### Keyboard Shortcuts
| Shortcut | Action |
|----------|--------|
| **CTRL+F12** | Toggle widget drag mode on/off |

---

## 📊 Available Widgets

### 1. **Relative Overlay** (`relative-overlay`)
Displays drivers around you sorted by track position, showing:
- Driver names, car numbers, and classes
- Live lap times and deltas
- Elo ratings and grades
- Stint information (laps completed, time)
- Pit status and damage indicators

**Configuration Options:**
- Drivers ahead/behind (default: 3 each)
- Toggle columns: Position, Class Color, Driver Name, Rating, Stint, Lap Time, Gap

### 2. **Standings Widget** (`standings`)
Full race leaderboard showing all drivers sorted by position:
- Overall race position
- Class colors and car numbers
- Positions gained/lost from start
- License class and iRating
- Gap to leader and interval
- Last lap times and delta to player
- Pit status indicators

**Gap to Competitors:**
- Missing advanced filtering (e.g., "In Pits", "Class Only") (Planned)

**Configuration Options:**
- Max drivers to display (default: 20)
- Toggle columns: Class Color, Car Number, Positions Gained, License, iRating, Car Brand, Interval, Gap, Last Lap, Delta, Pit Status

### 3. **Lap Timer Widget** (`lap-timer`)
Displays timing information for the current session:
- Current lap time (live)
- Last lap time
- Best lap time
- Delta to best lap (projected)
- Delta between last and best lap
- Current lap number / Total laps

**Configuration Options:**
- Toggle display of each timing component
- Update interval (default: 50ms)

### 4. **Fuel Calculator** (`fuel-calculator`)
Tracks fuel consumption and calculates pit strategy:
- Fuel remaining (liters and percentage)
- Fuel consumption per lap
- Laps remaining on current fuel
- Current lap / Total laps
- Fuel needed to finish race
- Fuel to add at next pit stop

**Gap to Competitors:**
- Missing "Refill amount" automation info (Planned)

**Configuration Options:**
- Fuel tank capacity (default: 110L)
- Update interval (default: 1000ms)

### 5. **Inputs Widget** (`inputs`)
Visualizes driver inputs in real-time:
- Steering wheel rotation indicator
- Throttle bar with percentage
- Brake bar with percentage
- Clutch bar with percentage
- Gear indicator
- Speed (km/h)

**Configuration Options:**
- Toggle individual input displays
- Update interval (default: 50ms)

### 6. **Input Trace Widget** (`input-trace`)
Historical trace of steering, throttle, and brake inputs:
- Steering trace line (blue)
- Throttle trace line (green)
- Brake trace line (red)
- Scrolling graph showing last N seconds

**Configuration Options:**
- History duration (default: 10 seconds)
- Update interval (default: 50ms)

### 7. **Track Map Widget** (`track-map`)
2D visualization of the track with car positions:
- Track outline
- Car positions as colored dots
- Player car highlighted
- Class-based color coding
- Sector markers

**Gap to Competitors:**
- Missing Zoom/Rotation features (Planned)

**Configuration Options:**
- Map size and zoom level
- Update interval (default: 100ms)

### 8. **Weather Widget** (`weather`)
Current and forecasted weather conditions:
- Current weather condition
- Track temperature
- Air temperature
- Wind speed and direction
- Rain probability
- Forecast for next hours

**Configuration Options:**
- Temperature units (Celsius/Fahrenheit)
- Update interval (default: 5000ms)

---

## 🛠️ Tech Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Runtime** | .NET | 10 |
| **UI Framework** | WPF | (Windows Presentation Foundation) |
| **MVVM Toolkit** | CommunityToolkit.Mvvm | 8.3.2 |
| **DI Container** | Microsoft.Extensions.DependencyInjection | 9.0.0 |
| **Hosting** | Microsoft.Extensions.Hosting | 9.0.0 |
| **Serialization** | System.Text.Json | 9.0.0 |
| **Testing** | xUnit, Moq, FluentAssertions | 2.9.2, 4.20.72, 6.12.1 |
| **Language** | C# 12 | (nullable reference types enforced) |

---

## 📁 Project Structure

```
RaceOverlay/
├── src/
│   ├── RaceOverlay.Core/              # Core interfaces and models (no WPF dependency)
│   │   ├── Widgets/                   # Widget contracts (IWidget, IWidgetConfiguration)
│   │   └── Providers/                 # Game provider contracts (IGameProvider, TelemetryData)
│   │
│   ├── RaceOverlay.Engine/            # WPF library — widget implementations and services
│   │   ├── Widgets/                   # Widget implementations (RelativeOverlay, Standings, etc.)
│   │   ├── Models/                    # Data models (RelativeDriver, StandingDriver, etc.)
│   │   ├── ViewModels/                # View models for overlay windows
│   │   └── Views/                     # XAML views for widgets
│   │
│   ├── RaceOverlay.App/               # WPF executable — main application
│   │   ├── MainWindow.xaml            # Control panel UI
│   │   ├── WidgetOverlayWindow.xaml   # Transparent overlay host
│   │   ├── Services/                  # Application services (WidgetDragService)
│   │   └── Themes/                    # Dark theme color palette
│   │
│   └── Providers/                     # Game-specific telemetry providers (scaffolded)
│       ├── RaceOverlay.Providers.iRacing/
│       ├── RaceOverlay.Providers.AssettoCorsa/
│       └── RaceOverlay.Providers.F124/
│
└── tests/
    ├── RaceOverlay.Core.Tests/
    ├── RaceOverlay.Engine.Tests/
    └── RaceOverlay.Providers.Tests/
```

### Dependency Flow
```
RaceOverlay.App (WPF executable)
  ├── RaceOverlay.Engine (widget implementations)
  │   └── RaceOverlay.Core (interfaces & contracts)
  └── RaceOverlay.Providers.* (game integrations)
      └── RaceOverlay.Core
```

---

## 🚦 Getting Started

### Prerequisites
- **.NET 10 SDK** (or later)
- **Visual Studio 2022+** or **JetBrains Rider**
- **Windows 10/11** (WPF requirement)

### Build & Run

```bash
# Clone the repository
git clone <repository-url>
cd RaceOverlay

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the application
dotnet run --project src/RaceOverlay.App

# Run tests
dotnet test
```

### Quick Start
1. Launch `RaceOverlay.App`
2. Select widgets from the control panel
3. Press **CTRL+F12** to enable drag mode
4. Position widgets as desired
5. Press **CTRL+F12** again to lock positions
6. Start your racing simulator (currently showing mock data)

---

## 🎨 Theme

RaceOverlay uses a custom dark theme with carefully selected colors:

| Resource Key | Color | Usage |
|--------------|-------|-------|
| `RO.BackgroundBrush` | #0B0F14 | Window backgrounds |
| `RO.SurfaceBrush` | #161B22 | Cards, panels |
| `RO.ForegroundBrush` | #E7EAF0 | Primary text |
| `RO.MutedBrush` | #9AA3B2 | Secondary text |
| `RO.PurpleBrush` | #6D28D9 | Primary accent |
| `RO.MagentaBrush` | #D946EF | Secondary accent |
| `RO.GreenBrush` | #10B981 | Positive / active states |
| `RO.RedBrush` | #EF4444 | Negative / warnings |
| `RO.BorderBrush` | #1E293B | Borders and dividers |

---

## 🏗️ Architecture

RaceOverlay follows a **plugin-based architecture** with clear separation of concerns:

### Core Concepts

#### 1. **Game Providers** (`IGameProvider`)
Abstract telemetry sources for different racing simulators. Each provider:
- Connects to game memory or UDP telemetry
- Normalizes data into `TelemetryData` events
- Handles connection lifecycle

**Status:** Scaffolded (iRacing, Assetto Corsa, F1 24)

#### 2. **Widgets** (`IWidget`)
Modular overlay components that:
- Receive telemetry data
- Render specific information
- Support custom configuration
- Manage their own lifecycle

**Current count:** 8 widgets implemented

#### 3. **Widget Registry** (`IWidgetRegistry`)
Service that:
- Discovers and registers available widgets
- Creates widget instances via DI
- Manages widget metadata

#### 4. **Telemetry Data** (`TelemetryData`)
Immutable data model containing:
- Speed, RPM, Gear
- Input states (Throttle, Brake, Clutch)
- Lap timing information
- Track and car metadata
- Timestamp for synchronization

### Design Patterns
- **MVVM**: Separation of UI and business logic
- **Dependency Injection**: Loose coupling via constructor injection
- **Observer**: Event-based telemetry distribution
- **Plugin/Registry**: Dynamic widget discovery and loading

---

## 🔧 Development

### Adding a New Widget

1. **Define configuration interface** in `RaceOverlay.Core/Widgets/`:
```csharp
public interface IMyWidgetConfig : IWidgetConfiguration
{
    int UpdateIntervalMs { get; set; }
    bool UseMockData { get; set; }
    double OverlayLeft { get; set; }
    double OverlayTop { get; set; }
    // ... custom properties
}
```

2. **Implement widget** in `RaceOverlay.Engine/Widgets/`:
```csharp
public class MyWidget : IWidget
{
    public string WidgetId => "my-widget";
    public string DisplayName => "My Widget";
    public string Description => "Description here";

    public async Task StartAsync(CancellationToken ct) { /* ... */ }
    public async Task StopAsync() { /* ... */ }
    public void UpdateConfiguration(IWidgetConfiguration config) { /* ... */ }
}
```

3. **Register widget** in `App.xaml.cs`:
```csharp
widgetRegistry.RegisterWidget(new WidgetMetadata
{
    WidgetId = "my-widget",
    DisplayName = "My Widget",
    WidgetType = typeof(MyWidget),
    ConfigurationType = typeof(IMyWidgetConfig)
});
```

### Adding a Game Provider

1. Create new project in `src/Providers/`
2. Reference `RaceOverlay.Core`
3. Implement `IGameProvider` interface
4. Connect to game-specific telemetry source
5. Emit `TelemetryData` events

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed development guide.

---

## 🗺️ Roadmap

### Phase 1: Core Foundation ✅
- [x] MVVM infrastructure with DI
- [x] Widget system architecture
- [x] Overlay window management
- [x] Widget drag/reposition
- [x] Basic widgets implementation

### Phase 2: Game Integration 🚧 (Current)
- [ ] iRacing telemetry provider
- [ ] Real telemetry data integration
- [ ] Session state management
- [ ] Multi-class race support

### Phase 3: Configuration & Persistence
- [ ] Widget configuration UI
- [ ] Position/layout saving (JSON)
- [ ] Per-game/track profiles
- [ ] Preset import/export

### Phase 4: Advanced Features
- [ ] Additional game providers (AC, F1 24)
- [ ] Advanced widgets (race engineer, telemetry graphs)
- [ ] Audio spotter integration
- [ ] Multi-monitor support

### Phase 5: Planned Widgets (Feature Parity)
- [ ] **Radar Widget** (Priority 1: Safety)
- [ ] **Head-to-Head (Battle) Widget** (Priority 1: Competition)
- [ ] **Pit Wall Widget** (Priority 1: Strategy)
- [ ] **Social/Stream Integration** (Chat / Recent Follows)
- [ ] **Driver Info & Stats** (Profile / Elo)

### Phase 6: Community & Ecosystem
- [ ] Plugin marketplace
- [ ] Community widget sharing
- [ ] API documentation
- [ ] Auto-update mechanism

---

## 📚 Documentation

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Detailed architecture guide
- **[PROJECT.md](PROJECT.md)** - Project overview and tech stack
- **[QUICKSTART.md](QUICKSTART.md)** - Quick setup guide
- **[HOTKEY_IMPLEMENTATION.md](HOTKEY_IMPLEMENTATION.md)** - Hotkey system details

---

## 🤝 Contributing

Contributions are welcome! Whether it's:
- Bug reports and fixes
- New widget implementations
- Game provider integrations
- Documentation improvements
- Feature suggestions

Please check existing issues and documentation before starting work.

---

## 📄 License

[License information to be added]

---

## 🎯 Comparison to RaceLabs

| Feature | RaceOverlay | RaceLabs |
|---------|-------------|----------|
| Relative Overlay | ✅ Mock data | ✅ |
| Standings | ✅ Mock data | ✅ |
| Fuel Calculator | ✅ Mock data | ✅ |
| Track Map | ✅ Mock data | ✅ |
| Inputs Display | ✅ Mock data | ✅ |
| Weather Widget | ✅ Mock data | ✅ |
| Lap Timer | ✅ Mock data | ✅ |
| Radar | 📅 Planned | ✅ |
| Head-to-Head | 📅 Planned | ✅ |
| Pit Wall | 📅 Planned | ✅ |
| Social/Stream | 📅 Planned | ✅ |
| Driver Info | 📅 Planned | ✅ |
| iRacing Support | 🚧 In progress | ✅ |
| Assetto Corsa | 📅 Planned | ✅ |
| F1 24 | 📅 Planned | ✅ |
| Open Source | ✅ | ❌ |
| Customizable | ✅ Fully | ⚠️ Limited |
| Free | ✅ | ⚠️ Freemium |

---

## 💬 Support

For questions, issues, or feature requests:
1. Check the documentation in the repo
2. Create an issue on GitHub
3. Contact the development team

---

**Built with ❤️ for the sim racing community**
