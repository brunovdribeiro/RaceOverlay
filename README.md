# RaceOverlay

Multi-game overlay management application for sim racing.

## Project Structure

- **RaceOverlay.Core** - Interfaces, models, and contracts
- **RaceOverlay.Engine** - Plugin management and orchestration
- **RaceOverlay.App** - WPF application (main UI)
- **RaceOverlay.Providers.*** - Game-specific telemetry providers
- **RaceOverlay.Modules.*** - Overlay modules (timing, radar, etc.)

## Getting Started

1. Open `RaceOverlay.sln` in Visual Studio 2022+ or Rider
2. Build the solution
3. Run `RaceOverlay.App`

## Adding a New Game Provider

1. Create a new class library in `src/Providers/`
2. Reference `RaceOverlay.Core`
3. Implement `IGameProvider`
4. Register in the Engine

## Tech Stack

- .NET 10
- WPF (Windows Presentation Foundation)
- xUnit (Testing)
- CommunityToolkit.Mvvm

## Architecture

This project follows a plugin-based architecture where each game is a separate provider that implements the `IGameProvider` interface.
