using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaceOverlay.App.ViewModels;
using RaceOverlay.Engine.Widgets;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Core.Widgets;

namespace RaceOverlay.App;

/// <summary>
/// Interaction logic for App.xaml
/// Configures dependency injection and manages application lifecycle.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    /// <summary>
    /// Called when the application starts.
    /// Initializes the DI container and shows the main window.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        // Build the host with DI configuration
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

        // Get the main window from DI and show it
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    /// <summary>
    /// Called when the application exits.
    /// Disposes of the host and cleans up resources.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    /// <summary>
    /// Configures all services for dependency injection.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Widget system
        services.AddSingleton<IWidgetRegistry>(sp =>
        {
            var registry = new WidgetRegistry();

            // Register Relative Overlay Widget
            registry.RegisterWidget(new WidgetMetadata
            {
                WidgetId = "relative-overlay",
                DisplayName = "Relative Overlay",
                Description = "Shows drivers around you with live lap times, stint information, and Elo ratings. Perfect for measuring pace and making smarter on-track decisions.",
                WidgetType = typeof(RelativeOverlay),
                ConfigurationType = typeof(RelativeOverlayConfig),
                Version = "1.0.0",
                Author = "RaceOverlay Team"
            });

            // Register Fuel Calculator Widget
            registry.RegisterWidget(new WidgetMetadata
            {
                WidgetId = "fuel-calculator",
                DisplayName = "Fuel Calculator",
                Description = "Tracks fuel remaining, consumption rate, and calculates fuel needed for pit stops.",
                WidgetType = typeof(FuelCalculator),
                ConfigurationType = typeof(FuelCalculatorConfig),
                Version = "1.0.0",
                Author = "RaceOverlay Team"
            });

            // Register Inputs Widget
            registry.RegisterWidget(new WidgetMetadata
            {
                WidgetId = "inputs",
                DisplayName = "Inputs",
                Description = "Visualizes throttle, brake, and steering telemetry in real time.",
                WidgetType = typeof(InputsWidget),
                ConfigurationType = typeof(InputsConfig),
                Version = "1.0.0",
                Author = "RaceOverlay Team"
            });

            // Register Input Trace Widget
            registry.RegisterWidget(new WidgetMetadata
            {
                WidgetId = "input-trace",
                DisplayName = "Input Trace",
                Description = "Scrolling line chart of throttle and brake inputs over time.",
                WidgetType = typeof(InputTraceWidget),
                ConfigurationType = typeof(InputTraceConfig),
                Version = "1.0.0",
                Author = "RaceOverlay Team"
            });

            // Register Standings Widget
            registry.RegisterWidget(new WidgetMetadata
            {
                WidgetId = "standings",
                DisplayName = "Standings",
                Description = "Full race leaderboard showing all drivers sorted by position with gaps, class colors, and player highlighting.",
                WidgetType = typeof(StandingsWidget),
                ConfigurationType = typeof(StandingsConfig),
                Version = "1.0.0",
                Author = "RaceOverlay Team"
            });

            return registry;
        });

        // Relative Overlay Widget
        services.AddTransient<RelativeOverlay>();
        services.AddTransient<RelativeOverlayViewModel>();
        services.AddTransient<RelativeOverlayView>();

        // Fuel Calculator Widget
        services.AddTransient<FuelCalculator>();
        services.AddTransient<FuelCalculatorViewModel>();
        services.AddTransient<FuelCalculatorView>();

        // Inputs Widget
        services.AddTransient<InputsWidget>();
        services.AddTransient<InputsViewModel>();
        services.AddTransient<InputsView>();

        // Input Trace Widget
        services.AddTransient<InputTraceWidget>();
        services.AddTransient<InputTraceViewModel>();
        services.AddTransient<InputTraceView>();

        // Standings Widget
        services.AddTransient<StandingsWidget>();
        services.AddTransient<StandingsViewModel>();
        services.AddTransient<StandingsView>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();

        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<WidgetOverlayWindow>();
    }}