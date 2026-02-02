# RaceOverlay Quick Start Guide

## Overview

This guide will help you get started with RaceOverlay development in just a few minutes.

## Prerequisites

- .NET 10 SDK (download from https://dotnet.microsoft.com/download)
- Visual Studio 2022 or VS Code with C# extension
- Windows 10/11

## Installation & First Run

### 1. Open the Project
```bash
cd c:\Sources\RaceOverlay
```

### 2. Build the Solution
```bash
dotnet build
```

Expected output:
```
RaceOverlay.App net10.0-windows succeeded
RaceOverlay.Engine net10.0 succeeded
RaceOverlay.Core net10.0 succeeded
```

### 3. Run the Application
```bash
dotnet run --project src/RaceOverlay.App/RaceOverlay.App.csproj
```

A window titled "RaceOverlay - Widget Manager" should open with:
- **Left panel:** Empty widget list (no widgets registered yet)
- **Right panel:** Empty configuration area
- **Bottom:** "Add Widget" and "Remove Widget" buttons

## Project Structure

```
src/
├── RaceOverlay.App/          ← WPF Application (Start Here!)
│   ├── App.xaml.cs           ← Dependency Injection Setup
│   ├── MainWindow.xaml       ← UI Layout
│   └── ViewModels/
│       └── MainWindowViewModel.cs
├── RaceOverlay.Core/         ← Interfaces & Data Models
│   └── Widgets/
│       ├── IWidget.cs
│       ├── IWidgetConfiguration.cs
│       └── WidgetMetadata.cs
└── RaceOverlay.Engine/       ← Core Services
    └── Widgets/
        ├── IWidgetRegistry.cs
        └── WidgetRegistry.cs
```

## Key Concepts

### 1. Widgets
Modular overlay components that display information. Examples:
- Timing Widget (lap times)
- Radar Widget (nearby cars)
- Input Display Widget (steering, throttle, brake)

### 2. Widget Registry
Service that manages widget registration and instantiation.

### 3. Dependency Injection
Automatic service resolution - services are injected into constructors.

## Common Tasks

### Creating Your First Widget

**Step 1:** Create a configuration interface in `src/RaceOverlay.Core/Widgets/`

```csharp
namespace RaceOverlay.Core.Widgets;

public interface IMyWidgetConfig : IWidgetConfiguration
{
    string ConfigurationType => "MyWidget";
}
```

**Step 2:** Create widget implementation in `src/RaceOverlay.App/`

```csharp
namespace RaceOverlay.App.Widgets;

public class MyWidget : IWidget
{
    public string WidgetId => "my-widget";
    public string DisplayName => "My Widget";
    public string Description => "My first widget";
    public IWidgetConfiguration Configuration { get; private set; }
    
    public Task StartAsync(CancellationToken ct = default)
    {
        // Initialize widget
        return Task.CompletedTask;
    }
    
    public Task StopAsync()
    {
        // Cleanup
        return Task.CompletedTask;
    }
    
    public void UpdateConfiguration(IWidgetConfiguration config)
    {
        Configuration = config;
    }
}
```

**Step 3:** Register in `App.xaml.cs`

```csharp
private static void ConfigureServices(IServiceCollection services)
{
    // Existing code...
    
    var metadata = new WidgetMetadata
    {
        WidgetId = "my-widget",
        DisplayName = "My Widget",
        Description = "My first widget",
        WidgetType = typeof(MyWidget),
        ConfigurationType = typeof(IMyWidgetConfig)
    };
    
    var registry = services.GetRequiredService<IWidgetRegistry>();
    registry.RegisterWidget(metadata);
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test file
dotnet test tests/RaceOverlay.Core.Tests/

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Building Release Version

```bash
dotnet build -c Release
```

Output will be in `src/RaceOverlay.App/bin/Release/net10.0-windows/`

## File Organization

### View Files (.xaml)
- Located in `src/RaceOverlay.App/`
- Define the UI layout
- Use data binding to connect to ViewModel

### ViewModel Files (.cs)
- Located in `src/RaceOverlay.App/ViewModels/`
- Contain application logic
- Use `[ObservableProperty]` and `[RelayCommand]` attributes

### Interface Files (.cs)
- Located in `src/RaceOverlay.Core/`
- Define contracts for plugins
- Enable loose coupling

### Service Files (.cs)
- Located in `src/RaceOverlay.Engine/`
- Implement core functionality
- Registered in DI container

## Important Files

| File | Purpose |
|------|---------|
| `ARCHITECTURE.md` | Comprehensive architecture guide |
| `IMPLEMENTATION_SUMMARY.md` | What was implemented and status |
| `App.xaml.cs` | Where services are registered |
| `MainWindow.xaml` | UI layout for main window |
| `MainWindowViewModel.cs` | Logic for main window |
| `IWidgetRegistry.cs` | Widget registration service interface |
| `WidgetRegistry.cs` | Widget registration service implementation |

## Troubleshooting

### "Project not found" error
Make sure you're in the correct directory:
```bash
cd c:\Sources\RaceOverlay
```

### Build fails with "Package not found"
Restore NuGet packages:
```bash
dotnet restore
```

### Application won't start
Check that all dependencies are installed:
```bash
dotnet build --verbose
```

### Port already in use
The application doesn't use network ports. If you get this error, restart your IDE.

## Development Tips

### Using the MVVM Toolkit

Observable properties auto-generate property changed notifications:
```csharp
[ObservableProperty]
private string name;  // Automatically implements INotifyPropertyChanged
```

Relay commands auto-generate command properties:
```csharp
[RelayCommand]
private void MyCommand()
{
    // Command logic
    // Accessible as: MyCommandCommand property
}
```

### Data Binding in XAML

```xaml
<TextBlock Text="{Binding PropertyName}"/>
<Button Command="{Binding MyCommandCommand}"/>
```

### Debugging

Set breakpoints in:
- ViewModel methods (double-click line number)
- Command handlers (when buttons are clicked)
- Property setters (when UI updates)

Use Debug → Windows → Output to see logs.

## Useful Commands

```bash
# Clean all build outputs
dotnet clean

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Build with verbosity
dotnet build -v detailed

# Run application
dotnet run --project src/RaceOverlay.App/RaceOverlay.App.csproj

# Run tests
dotnet test

# List projects in solution
dotnet sln list

# Add new project
dotnet new classlib -n RaceOverlay.NewModule
dotnet sln add src/RaceOverlay.NewModule/RaceOverlay.NewModule.csproj
```

## Next Steps

1. **Read** [ARCHITECTURE.md](ARCHITECTURE.md) for detailed design
2. **Create** your first widget using the example above
3. **Run** the tests to verify everything works
4. **Implement** a game provider (iRacing, Assetto Corsa, or F1)
5. **Build** overlay windows to display widgets

## Getting Help

- Check `ARCHITECTURE.md` for comprehensive documentation
- Review `IMPLEMENTATION_SUMMARY.md` for what's been built
- Look at existing code examples in the project
- Check the troubleshooting section in ARCHITECTURE.md

## Contributing

When adding new features:
1. Create a branch: `git checkout -b feature/my-feature`
2. Make changes following the code style guide
3. Add/update unit tests
4. Update documentation
5. Submit pull request

---

**Happy Coding!** 🚗💨

For detailed information, see [ARCHITECTURE.md](ARCHITECTURE.md)
