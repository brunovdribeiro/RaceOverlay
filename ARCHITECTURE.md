# RaceOverlay Project Architecture Guide

**Last Updated:** February 2, 2026  
**Project Status:** Initial Implementation Phase  
**.NET Version:** .NET 10  
**UI Framework:** WPF (Windows Presentation Foundation)

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Core Concepts](#core-concepts)
4. [Project Structure](#project-structure)
5. [Widget System](#widget-system)
6. [Dependency Injection Setup](#dependency-injection-setup)
7. [MVVM Pattern Implementation](#mvvm-pattern-implementation)
8. [Building and Running](#building-and-running)
9. [Development Workflow](#development-workflow)
10. [Future Enhancements](#future-enhancements)

---

## Project Overview

**RaceOverlay** is a modular overlay system for racing simulators (iRacing, Assetto Corsa, F1 2024, etc.) that displays real-time telemetry and custom widgets on top of the game window.

### Key Design Principles

- **Plugin Architecture**: Game providers and widgets are discovered and loaded dynamically
- **Separation of Concerns**: Clear boundaries between UI, business logic, and game integration
- **MVVM Pattern**: Modern WPF architecture using CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection for loose coupling
- **Immutability**: Data models use init-only properties for thread safety

### Target Users

- Sim racing enthusiasts who want customizable overlays
- Game integration developers
- Telemetry system designers

---

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────┐
│         RaceOverlay.App (WPF)               │
│  - MainWindow (View)                        │
│  - MainWindowViewModel (ViewModel)          │
│  - Dependency Injection Configuration       │
└────────────┬────────────────────────────────┘
             │
     ┌───────┴────────┐
     │                │
┌────▼──────────┐  ┌──▼─────────────────┐
│ RaceOverlay   │  │ RaceOverlay.Engine  │
│ .Core         │  │ - Widget Registry   │
│ - Interfaces  │  │ - Service Container │
│ - Models      │  │ - Plugin Discovery  │
└────────┬──────┘  └──┬──────────────────┘
         │           │
    ┌────┴───────────┴──────┐
    │   Provider Plugins    │
    ├──────────────────────┤
    │ - iRacing            │
    │ - Assetto Corsa      │
    │ - F1 2024            │
    └──────────────────────┘
```

### Technology Stack

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **UI Framework** | WPF | .NET 10 | Desktop application UI |
| **MVVM Toolkit** | CommunityToolkit.Mvvm | 8.3.2 | Observable properties, commands |
| **Dependency Injection** | Microsoft.Extensions.DependencyInjection | 9.0.0 | Service registration and resolution |
| **Hosting** | Microsoft.Extensions.Hosting | 9.0.0 | Application lifecycle management |
| **Logging** | Microsoft.Extensions.Logging | 9.0.0 | Structured logging |
| **Testing** | xUnit | 2.9.2 | Unit test framework |
| **Mocking** | Moq | 4.20.72 | Object mocking for tests |

---

## Core Concepts

### 1. Game Providers

**Purpose**: Abstraction layer for different racing simulators

**Interface**: `IGameProvider` (RaceOverlay.Core)

```csharp
public interface IGameProvider
{
    string GameId { get; }                    // Unique ID (e.g., "iRacing")
    string DisplayName { get; }                // Display name for UI
    
    bool IsGameRunning();                      // Check if game is active
    Task StartAsync(CancellationToken ct);    // Start receiving telemetry
    Task StopAsync();                          // Stop receiving telemetry
    
    event EventHandler<TelemetryData>? DataReceived;  // Telemetry stream
}
```

**Responsibilities**:
- Connect to the game's memory or API
- Parse and normalize telemetry data
- Emit data events at regular intervals
- Handle connection failures gracefully

**Current Implementations**: None (placeholders exist)

### 2. Telemetry Data

**Purpose**: Immutable data model for real-time race telemetry

**Class**: `TelemetryData` (RaceOverlay.Core)

Contains all real-time information:
- Speed, RPM, Gear
- Input states (Throttle, Brake, Clutch)
- Lap information (current, best, last lap times)
- Lap number, Track name, Car name
- Timestamp for sync purposes

**Properties are init-only**: Ensures thread safety and immutability across the application

### 3. Widgets

**Purpose**: Modular overlay components that display specific information

**Interfaces**:

- `IWidget`: Represents a running widget instance
- `IWidgetConfiguration`: Base interface for widget-specific configuration
- `WidgetMetadata`: Metadata describing an available widget type

**Widget Lifecycle**:

1. **Registration**: Widget metadata is registered with `IWidgetRegistry`
2. **Discovery**: UI discovers available widgets via the registry
3. **Instantiation**: User selects a widget → `CreateWidget()` creates an instance
4. **Activation**: Widget's `StartAsync()` begins listening for telemetry
5. **Deactivation**: Widget's `StopAsync()` stops processing
6. **Removal**: Widget is disposed and removed from active widgets

**Example Widget Types** (To be implemented):
- **Timing Widget**: Displays lap times, delta, splits
- **Radar Widget**: Shows nearby cars
- **Input Display Widget**: Visualizes throttle, brake, steering

### 4. Widget Registry

**Purpose**: Service that manages widget registration, discovery, and instantiation

**Interface**: `IWidgetRegistry` (RaceOverlay.Engine)

**Implementation**: `WidgetRegistry` (RaceOverlay.Engine)

**Responsibilities**:
- Store widget metadata
- Create widget instances via dependency injection
- Manage widget lifecycle
- Support hot-reloading (future enhancement)

---

## Project Structure

### Directory Layout

```
RaceOverlay/
├── src/
│   ├── RaceOverlay.Core/              # Core interfaces and models
│   │   ├── Widgets/                   # Widget-related interfaces
│   │   │   ├── IWidget.cs
│   │   │   ├── IWidgetConfiguration.cs
│   │   │   └── WidgetMetadata.cs
│   │   ├── Providers/                 # Game provider interfaces
│   │   │   ├── IGameProvider.cs
│   │   │   └── TelemetryData.cs
│   │   └── RaceOverlay.Core.csproj
│   │
│   ├── RaceOverlay.Engine/            # Engine services
│   │   ├── Widgets/
│   │   │   ├── IWidgetRegistry.cs
│   │   │   └── WidgetRegistry.cs
│   │   └── RaceOverlay.Engine.csproj
│   │
│   ├── RaceOverlay.App/               # WPF Application
│   │   ├── ViewModels/
│   │   │   └── MainWindowViewModel.cs
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   └── RaceOverlay.App.csproj
│   │
│   ├── Providers/                     # Game provider implementations
│   │   ├── RaceOverlay.Providers.iRacing/
│   │   ├── RaceOverlay.Providers.AssettoCorsa/
│   │   └── RaceOverlay.Providers.F124/
│   │
│   └── Modules/                       # Widget modules (future)
│       ├── RaceOverlay.Modules.Timing/
│       ├── RaceOverlay.Modules.Radar/
│       └── RaceOverlay.Modules.InputDisplay/
│
├── tests/
│   ├── RaceOverlay.Core.Tests/
│   ├── RaceOverlay.Engine.Tests/
│   └── RaceOverlay.Providers.Tests/
│
├── Directory.Build.props               # Common build settings
├── Directory.Packages.props             # Central package management
└── RaceOverlay.slnx                    # Solution file
```

### Project Dependencies

```
RaceOverlay.App (WPF)
  ├── RaceOverlay.Core
  └── RaceOverlay.Engine
       └── RaceOverlay.Core

RaceOverlay.Providers.*
  └── RaceOverlay.Core

RaceOverlay.Modules.* (future)
  └── RaceOverlay.Core
```

---

## Widget System

### Widget Architecture

A widget consists of three components:

1. **Metadata** (`WidgetMetadata`)
   - Static information about the widget
   - Type references for instantiation
   - Display information for UI

2. **Implementation** (`IWidget`)
   - The actual widget logic
   - Processes telemetry data
   - Manages lifecycle

3. **Configuration** (`IWidgetConfiguration`)
   - Widget-specific settings
   - Serializable for persistence
   - Type varies per widget

### Creating a New Widget

**Step 1: Define Configuration Interface** (in RaceOverlay.Core)

```csharp
namespace RaceOverlay.Core.Widgets;

public interface ITimingWidgetConfig : IWidgetConfiguration
{
    string ConfigurationType => "Timing";
    
    bool ShowDelta { get; init; }
    bool ShowSplits { get; init; }
    int FontSize { get; init; }
}
```

**Step 2: Implement Widget** (in module project)

```csharp
namespace RaceOverlay.Modules.Timing;

public class TimingWidget : IWidget
{
    public string WidgetId => "timing-widget";
    public string DisplayName => "Timing";
    public string Description => "Displays lap times and delta";
    
    public IWidgetConfiguration Configuration { get; private set; }
    
    public async Task StartAsync(CancellationToken ct = default)
    {
        // Subscribe to telemetry, start rendering, etc.
    }
    
    public async Task StopAsync()
    {
        // Clean up resources
    }
    
    public void UpdateConfiguration(IWidgetConfiguration config)
    {
        Configuration = config;
        // Re-render with new configuration
    }
}
```

**Step 3: Register Widget** (in App.xaml.cs or HostBuilder configuration)

```csharp
// In ConfigureServices method
var timingMetadata = new WidgetMetadata
{
    WidgetId = "timing-widget",
    DisplayName = "Timing",
    Description = "Displays lap times and delta",
    WidgetType = typeof(TimingWidget),
    ConfigurationType = typeof(ITimingWidgetConfig),
    Version = "1.0.0",
    Author = "RaceOverlay Team"
};

var widgetRegistry = services.GetRequiredService<IWidgetRegistry>();
widgetRegistry.RegisterWidget(timingMetadata);
```

---

## Dependency Injection Setup

### DI Container Configuration

**File**: `RaceOverlay.App/App.xaml.cs`

The application uses `Microsoft.Extensions.Hosting` to configure the DI container:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    _host = Host.CreateDefaultBuilder()
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
        })
        .ConfigureServices((context, services) =>
        {
            ConfigureServices(services);
        })
        .Build();

    var mainWindow = _host.Services.GetRequiredService<MainWindow>();
    mainWindow.Show();
    
    base.OnStartup(e);
}

private static void ConfigureServices(IServiceCollection services)
{
    // Widget system
    services.AddSingleton<IWidgetRegistry, WidgetRegistry>();

    // ViewModels
    services.AddTransient<MainWindowViewModel>();

    // Views
    services.AddTransient<MainWindow>();
}
```

### Service Lifetimes

- **Singleton**: `IWidgetRegistry` - Single instance for entire application
- **Transient**: `MainWindowViewModel`, `MainWindow` - New instance each time

### Constructor Injection

Services are injected via constructor:

```csharp
public class MainWindowViewModel : ObservableObject
{
    private readonly IWidgetRegistry _widgetRegistry;
    
    public MainWindowViewModel(IWidgetRegistry widgetRegistry)
    {
        _widgetRegistry = widgetRegistry ?? throw new ArgumentNullException(nameof(widgetRegistry));
    }
}
```

---

## MVVM Pattern Implementation

### Using CommunityToolkit.Mvvm

The project uses **CommunityToolkit.Mvvm** (formerly MVVM Toolkit) for:

1. **Observable Properties**: Automatic `INotifyPropertyChanged` implementation
2. **Relay Commands**: Simplified command implementation
3. **Source Generators**: Compile-time code generation for performance

### MainWindowViewModel Structure

```csharp
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IWidgetRegistry _widgetRegistry;
    
    // Observable property (auto-generates OnPropertyChanged)
    [ObservableProperty]
    private WidgetMetadata? selectedWidget;
    
    [ObservableProperty]
    private ObservableCollection<WidgetMetadata> availableWidgets = new();
    
    // Relay command (auto-generates Command property)
    [RelayCommand]
    private void AddWidget()
    {
        // Implementation
    }
    
    [RelayCommand]
    private void RemoveWidget()
    {
        // Implementation
    }
}
```

### XAML Data Binding

The MainWindow binds to ViewModel properties and commands:

```xaml
<ListBox ItemsSource="{Binding AvailableWidgets}" 
         SelectedItem="{Binding SelectedWidget}"/>

<Button Command="{Binding AddWidgetCommand}" 
        Content="Add Widget"/>
```

### Key Benefits

- **Less Boilerplate**: No manual `OnPropertyChanged` calls
- **Type Safety**: Commands are strongly typed
- **Performance**: Source generators eliminate reflection overhead
- **Testability**: Easy to mock and test ViewModels

---

## Building and Running

### Prerequisites

- **.NET 10 SDK** (minimum)
- **Visual Studio 2022** or **VS Code** with C# extensions
- **Windows 10/11** (for WPF)

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Build specific project
dotnet build src/RaceOverlay.App/RaceOverlay.App.csproj

# Build with release configuration
dotnet build -c Release
```

### Running the Application

```bash
# Run the WPF app
dotnet run --project src/RaceOverlay.App/RaceOverlay.App.csproj

# Run tests
dotnet test
```

### Build Artifacts

- **Binaries**: `**/bin/Debug/net10.0*/`
- **Object Files**: `**/obj/Debug/`
- **Unit Tests**: `tests/*/bin/Debug/net10.0/`

---

## Development Workflow

### Adding a New Feature

#### 1. Create Core Interfaces

Define new interfaces in `RaceOverlay.Core`:

```bash
src/RaceOverlay.Core/[FeatureName]/I[Feature].cs
```

#### 2. Implement Engine Services

Add services to `RaceOverlay.Engine`:

```bash
src/RaceOverlay.Engine/[FeatureName]/[Feature].cs
```

#### 3. Add UI Components (if needed)

Create Views and ViewModels in `RaceOverlay.App`:

```bash
src/RaceOverlay.App/ViewModels/[Feature]ViewModel.cs
src/RaceOverlay.App/Views/[Feature]View.xaml
```

#### 4. Register Services

Update `App.xaml.cs` `ConfigureServices` method:

```csharp
services.AddSingleton<INewService, NewService>();
services.AddTransient<NewViewModel>();
```

#### 5. Write Tests

Create unit tests in `tests/RaceOverlay.[Project].Tests/`:

```bash
tests/RaceOverlay.Core.Tests/[FeatureName]Tests.cs
tests/RaceOverlay.Engine.Tests/[FeatureName]Tests.cs
```

### Code Style Guidelines

- **Naming**: PascalCase for public members, camelCase for private
- **Null Safety**: Enable nullable reference types (`<Nullable>enable</Nullable>`)
- **Async**: Use async/await for I/O operations
- **Error Handling**: Use custom exceptions, document in XML comments
- **Git**: Commit messages should be descriptive (e.g., "feat: Add timing widget")

### Testing Strategy

1. **Unit Tests**: Test individual classes in isolation
2. **Integration Tests**: Test interaction between components
3. **UI Tests**: Manual testing (automated UI testing is complex with WPF)

Example test:

```csharp
[Fact]
public void RegisterWidget_ValidMetadata_Succeeds()
{
    // Arrange
    var registry = new WidgetRegistry();
    var metadata = new WidgetMetadata
    {
        WidgetId = "test",
        DisplayName = "Test",
        Description = "Test widget",
        WidgetType = typeof(TestWidget),
        ConfigurationType = typeof(TestConfig)
    };
    
    // Act
    registry.RegisterWidget(metadata);
    
    // Assert
    Assert.Contains(metadata, registry.GetRegisteredWidgets());
}
```

---

## Future Enhancements

### Planned Features

#### 1. Widget Persistence
- **Save/Load Configurations**: JSON-based widget configuration persistence
- **Layout Saving**: Remember widget positions and sizes across sessions
- **Implementation**: Use `System.Text.Json` with `AppData` folder storage

#### 2. Game Provider Implementations
- **iRacing Integration**: Connect to iRacing telemetry plugin
- **Assetto Corsa**: Implement UDP telemetry listener
- **F1 2024**: Use official UDP specification
- **Status**: Currently empty scaffolds

#### 3. Module System
- **Plugin Discovery**: Scan folders for widget modules
- **Hot-Reloading**: Load/unload widgets without restart
- **Distribution**: NuGet packages for 3rd-party widgets

#### 4. Overlay Windows
- **Transparent Windows**: Draw overlays on top of game
- **Performance Optimization**: Minimal CPU/GPU usage
- **Input Pass-Through**: Allow game to receive input under overlay

#### 5. Advanced Configuration
- **Property Grid**: Generic UI for widget configuration
- **Presets**: Save/load widget configurations
- **Profiles**: Different widget sets per game/track

#### 6. Data Visualization
- **Charts**: Historical data visualization (fuel, tire temps)
- **Graphs**: Real-time performance monitoring
- **Animations**: Smooth transitions and effects

### Architecture Evolution

```
Current:
RaceOverlay.App → Engine → Core

Planned:
RaceOverlay.App (Desktop)
├─ RaceOverlay.Engine (Core services)
│  ├─ IWidgetRegistry
│  ├─ IGameProviderRegistry  (NEW)
│  ├─ IOverlayManager        (NEW)
│  └─ IConfigurationService  (NEW)
└─ RaceOverlay.Core
   ├─ IWidget, IWidgetConfiguration
   ├─ IGameProvider, TelemetryData
   ├─ IConfigurable           (NEW)
   └─ IAudioService           (NEW)

Third-Party:
- RaceOverlay.Modules.Timing
- RaceOverlay.Modules.Radar
- RaceOverlay.Providers.iRacing
```

---

## Troubleshooting

### Build Issues

**Error**: "The name 'ActivatorUtilities' does not exist"
**Solution**: Ensure `Microsoft.Extensions.DependencyInjection` package reference is added to the project.

**Error**: "The property 'X' does not exist in XML namespace"
**Solution**: Verify WPF control is used correctly (e.g., TextBlock doesn't support Padding, use Border with TextBlock inside).

### Runtime Issues

**Error**: "No service for type 'IWidget Registry' has been registered"
**Solution**: Verify `services.AddSingleton<IWidgetRegistry, WidgetRegistry>()` is called in `ConfigureServices`.

**Widget Not Appearing**: Ensure widget is registered before `MainWindow` is displayed.

---

## References

### Documentation
- [MVVM Toolkit Documentation](https://learn.microsoft.com/en-us/windows/communitytoolkit/mvvm/)
- [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [WPF Data Binding](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/)

### Tools
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [.NET CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/)
- [Git](https://git-scm.com/)

### Related Standards
- [iRacing Telemetry](https://forums.iracing.com/discussion/7435/telemetry-in-iracing)
- [Assetto Corsa UDP Protocol](https://www.assettocorsamods.com/threads/rf2-documented-telemetry-streaming-via-udp.51/)

---

## Contact & Support

For questions or issues:
1. Check the [Troubleshooting](#troubleshooting) section
2. Review the [Development Workflow](#development-workflow) guide
3. Create an issue in the project repository
4. Contact the RaceOverlay Team

---

**Document Version**: 1.0.0  
**Last Reviewed**: February 2, 2026
