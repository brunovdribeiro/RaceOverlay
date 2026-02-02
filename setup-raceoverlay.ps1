# RaceOverlay Project Generator for .NET 10
# Run this script in your desired root directory

Write-Host "Creating RaceOverlay Solution Structure..." -ForegroundColor Cyan

# Create solution
dotnet new sln -n RaceOverlay

# Create directories
New-Item -ItemType Directory -Force -Path "src"
New-Item -ItemType Directory -Force -Path "tests"
New-Item -ItemType Directory -Force -Path "src/Providers"
New-Item -ItemType Directory -Force -Path "src/Modules"

# ===== CORE PROJECTS =====
Write-Host "`nCreating Core Projects..." -ForegroundColor Green

# 1. Core SDK (Interfaces, Models, Contracts)
dotnet new classlib -n RaceOverlay.Core -o src/RaceOverlay.Core -f net10.0
dotnet sln add src/RaceOverlay.Core/RaceOverlay.Core.csproj

# 2. Engine (Orchestration, Plugin Management)
dotnet new classlib -n RaceOverlay.Engine -o src/RaceOverlay.Engine -f net10.0
dotnet sln add src/RaceOverlay.Engine/RaceOverlay.Engine.csproj
dotnet add src/RaceOverlay.Engine/RaceOverlay.Engine.csproj reference src/RaceOverlay.Core/RaceOverlay.Core.csproj

# 3. WPF Application
dotnet new wpf -n RaceOverlay.App -o src/RaceOverlay.App -f net10.0-windows
dotnet sln add src/RaceOverlay.App/RaceOverlay.App.csproj
dotnet add src/RaceOverlay.App/RaceOverlay.App.csproj reference src/RaceOverlay.Engine/RaceOverlay.Engine.csproj
dotnet add src/RaceOverlay.App/RaceOverlay.App.csproj reference src/RaceOverlay.Core/RaceOverlay.Core.csproj

# ===== GAME PROVIDER PROJECTS =====
Write-Host "`nCreating Game Provider Projects..." -ForegroundColor Green

# iRacing Provider
dotnet new classlib -n RaceOverlay.Providers.iRacing -o src/Providers/RaceOverlay.Providers.iRacing -f net10.0
dotnet sln add src/Providers/RaceOverlay.Providers.iRacing/RaceOverlay.Providers.iRacing.csproj
dotnet add src/Providers/RaceOverlay.Providers.iRacing/RaceOverlay.Providers.iRacing.csproj reference src/RaceOverlay.Core/RaceOverlay.Core.csproj

# Assetto Corsa Provider
dotnet new classlib -n RaceOverlay.Providers.AssettoCorsa -o src/Providers/RaceOverlay.Providers.AssettoCorsa -f net10.0
dotnet sln add src/Providers/RaceOverlay.Providers.AssettoCorsa/RaceOverlay.Providers.AssettoCorsa.csproj
dotnet add src/Providers/RaceOverlay.Providers.AssettoCorsa/RaceOverlay.Providers.AssettoCorsa.csproj reference src/RaceOverlay.Core/RaceOverlay.Core.csproj

# F1 24 Provider
dotnet new classlib -n RaceOverlay.Providers.F124 -o src/Providers/RaceOverlay.Providers.F124 -f net10.0
dotnet sln add src/Providers/RaceOverlay.Providers.F124/RaceOverlay.Providers.F124.csproj
dotnet add src/Providers/RaceOverlay.Providers.F124/RaceOverlay.Providers.F124.csproj reference src/RaceOverlay.Core/RaceOverlay.Core.csproj

# ===== OVERLAY MODULE PROJECTS =====
Write-Host "`nCreating Overlay Module Projects..." -ForegroundColor Green

# Timing Module
dotnet new classlib -n RaceOverlay.Modules.Timing -o src/Modules/RaceOverlay.Modules.Timing -f net10.0-windows
dotnet sln add src/Modules/RaceOverlay.Modules.Timing/RaceOverlay.Modules.Timing.csproj
dotnet add src/Modules/RaceOverlay.Modules.Timing/RaceOverlay.Modules.Timing.csproj reference src/RaceOverlay.Core/RaceOverlay.Core.csproj

# Radar Module
dotnet new classlib -n RaceOverlay.Modules.Radar -o src/Modules/RaceOverlay.Modules.Radar -f net10.0-windows
dotnet sln add src/Modules/RaceOverlay.Modules.Radar/RaceOverlay.Modules.Radar.csproj
dotnet add src/Modules/RaceOverlay.Modules.Radar/RaceOverlay.Modules.Radar.csproj reference src/RaceOverlay.Core/RaceOverlay.Core.csproj

# Input Display Module
dotnet new classlib -n RaceOverlay.Modules.InputDisplay -o src/Modules/RaceOverlay.Modules.InputDisplay -f net10.0-windows
dotnet sln add src/Modules/RaceOverlay.Modules.InputDisplay/RaceOverlay.Modules.InputDisplay.csproj
dotnet add src/Modules/RaceOverlay.Modules.InputDisplay/RaceOverlay.Modules.InputDisplay.csproj reference src/RaceOverlay.Core/RaceOverlay.Core.csproj

# ===== TEST PROJECTS =====
Write-Host "`nCreating Test Projects..." -ForegroundColor Green

# Core Tests
dotnet new xunit -n RaceOverlay.Core.Tests -o tests/RaceOverlay.Core.Tests -f net10.0
dotnet sln add tests/RaceOverlay.Core.Tests/RaceOverlay.Core.Tests.csproj
dotnet add tests/RaceOverlay.Core.Tests/RaceOverlay.Core.Tests.csproj reference src/RaceOverlay.Core/RaceOverlay.Core.csproj

# Engine Tests
dotnet new xunit -n RaceOverlay.Engine.Tests -o tests/RaceOverlay.Engine.Tests -f net10.0
dotnet sln add tests/RaceOverlay.Engine.Tests/RaceOverlay.Engine.Tests.csproj
dotnet add tests/RaceOverlay.Engine.Tests/RaceOverlay.Engine.Tests.csproj reference src/RaceOverlay.Engine/RaceOverlay.Engine.csproj

# Provider Tests
dotnet new xunit -n RaceOverlay.Providers.Tests -o tests/RaceOverlay.Providers.Tests -f net10.0
dotnet sln add tests/RaceOverlay.Providers.Tests/RaceOverlay.Providers.Tests.csproj
dotnet add tests/RaceOverlay.Providers.Tests/RaceOverlay.Providers.Tests.csproj reference src/Providers/RaceOverlay.Providers.iRacing/RaceOverlay.Providers.iRacing.csproj
dotnet add tests/RaceOverlay.Providers.Tests/RaceOverlay.Providers.Tests.csproj reference src/Providers/RaceOverlay.Providers.AssettoCorsa/RaceOverlay.Providers.AssettoCorsa.csproj

# ===== CREATE CENTRAL PACKAGE MANAGEMENT =====
Write-Host "`nCreating Directory.Packages.props..." -ForegroundColor Green

@"
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  
  <ItemGroup>
    <!-- Testing -->
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageVersion Include="Moq" Version="4.20.72" />
    <PackageVersion Include="FluentAssertions" Version="6.12.1" />
    
    <!-- MVVM & WPF -->
    <PackageVersion Include="CommunityToolkit.Mvvm" Version="8.3.2" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="9.0.0" />
    
    <!-- JSON & Serialization -->
    <PackageVersion Include="System.Text.Json" Version="9.0.0" />
    
    <!-- Memory & Performance -->
    <PackageVersion Include="System.Memory" Version="4.6.0" />
  </ItemGroup>
</Project>
"@ | Out-File -FilePath "Directory.Packages.props" -Encoding UTF8

# ===== CREATE DIRECTORY.BUILD.PROPS =====
Write-Host "Creating Directory.Build.props..." -ForegroundColor Green

@"
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <WarningsAsErrors>nullable</WarningsAsErrors>
    <Authors>RaceOverlay Team</Authors>
    <Company>RaceOverlay</Company>
    <Copyright>Copyright © $(Company) $([System.DateTime]::Now.Year)</Copyright>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryType>git</RepositoryType>
  </PropertyGroup>
</Project>
"@ | Out-File -FilePath "Directory.Build.props" -Encoding UTF8

# ===== CREATE BASE INTERFACES =====
Write-Host "`nCreating Base Interfaces in Core..." -ForegroundColor Green

# IGameProvider Interface
@"
namespace RaceOverlay.Core.Providers;

/// <summary>
/// Contract for game telemetry providers
/// </summary>
public interface IGameProvider
{
    /// <summary>
    /// Unique identifier for this game (e.g., "iRacing", "AssettoCorsa")
    /// </summary>
    string GameId { get; }

    /// <summary>
    /// Display name of the game
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Check if the game is currently running
    /// </summary>
    bool IsGameRunning();

    /// <summary>
    /// Start capturing telemetry data
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop capturing telemetry data
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Fired when new telemetry data is available
    /// </summary>
    event EventHandler<TelemetryData>? DataReceived;
}
"@ | Out-File -FilePath "src/RaceOverlay.Core/IGameProvider.cs" -Encoding UTF8

# TelemetryData Model
@"
namespace RaceOverlay.Core.Providers;

/// <summary>
/// Base telemetry data model
/// </summary>
public class TelemetryData
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public float Speed { get; init; }
    public float Rpm { get; init; }
    public int Gear { get; init; }
    public float Throttle { get; init; }
    public float Brake { get; init; }
    public float Clutch { get; init; }
    public TimeSpan? CurrentLapTime { get; init; }
    public TimeSpan? LastLapTime { get; init; }
    public TimeSpan? BestLapTime { get; init; }
    public int LapNumber { get; init; }
    public string? TrackName { get; init; }
    public string? CarName { get; init; }
}
"@ | Out-File -FilePath "src/RaceOverlay.Core/TelemetryData.cs" -Encoding UTF8

# Delete default Class1.cs files
Remove-Item "src/RaceOverlay.Core/Class1.cs" -ErrorAction SilentlyContinue
Remove-Item "src/RaceOverlay.Engine/Class1.cs" -ErrorAction SilentlyContinue
Remove-Item "src/Providers/*/Class1.cs" -ErrorAction SilentlyContinue
Remove-Item "src/Modules/*/Class1.cs" -ErrorAction SilentlyContinue

# ===== CREATE README =====
Write-Host "`nCreating README..." -ForegroundColor Green

@"
# RaceOverlay

Multi-game overlay management application for sim racing.

## Project Structure

- **RaceOverlay.Core** - Interfaces, models, and contracts
- **RaceOverlay.Engine** - Plugin management and orchestration
- **RaceOverlay.App** - WPF application (main UI)
- **RaceOverlay.Providers.*** - Game-specific telemetry providers
- **RaceOverlay.Modules.*** - Overlay modules (timing, radar, etc.)

## Getting Started

1. Open ``RaceOverlay.sln`` in Visual Studio 2022+ or Rider
2. Build the solution
3. Run ``RaceOverlay.App``

## Adding a New Game Provider

1. Create a new class library in ``src/Providers/``
2. Reference ``RaceOverlay.Core``
3. Implement ``IGameProvider``
4. Register in the Engine

## Tech Stack

- .NET 10
- WPF (Windows Presentation Foundation)
- xUnit (Testing)
- CommunityToolkit.Mvvm

## Architecture

This project follows a plugin-based architecture where each game is a separate provider that implements the ``IGameProvider`` interface.
"@ | Out-File -FilePath "README.md" -Encoding UTF8

# ===== CREATE .GITIGNORE =====
Write-Host "Creating .gitignore..." -ForegroundColor Green

@"
## Ignore Visual Studio temporary files, build results, and
## files generated by popular Visual Studio add-ons.

# User-specific files
*.rsuser
*.suo
*.user
*.userosscache
*.sln.docstates

# Build results
[Dd]ebug/
[Dd]ebugPublic/
[Rr]elease/
[Rr]eleases/
x64/
x86/
[Ww][Ii][Nn]32/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
bld/
[Bb]in/
[Oo]bj/
[Ll]og/
[Ll]ogs/

# Visual Studio cache/options directory
.vs/

# Rider
.idea/

# JetBrains Rider
*.sln.iml
"@ | Out-File -FilePath ".gitignore" -Encoding UTF8

Write-Host "`n✅ Solution created successfully!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Open RaceOverlay.sln in Visual Studio 2022 or later"
Write-Host "2. Build the solution"
Write-Host "3. Start implementing your game providers!"
Write-Host "`nHappy coding! 🏎️💨" -ForegroundColor Cyan