using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaceOverlay.App.ViewModels;
using RaceOverlay.Core.Providers;
using RaceOverlay.Core.Services;
using RaceOverlay.Engine.Factories;
using RaceOverlay.Engine.Widgets;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Providers.iRacing;
using Serilog;
using Velopack;

namespace RaceOverlay.App;

/// <summary>
/// Interaction logic for App.xaml
/// Configures dependency injection and manages application lifecycle.
/// </summary>
public partial class App : Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }

    private IHost? _host;
    private IRacingDataService? _dataService;

    /// <summary>
    /// Called when the application starts.
    /// Initializes the DI container and shows the main window.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        SetupExceptionHandling();

        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RaceOverlay", "logs", "raceoverlay-.log");

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information();

#if DEBUG
        loggerConfig = loggerConfig.WriteTo.Console();
#endif

        Log.Logger = loggerConfig
            .WriteTo.File(logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("RaceOverlay starting up");

        try
        {
            // Build the host with DI configuration
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();

            // Start the iRacing data service
            _dataService = _host.Services.GetRequiredService<IRacingDataService>();
            _dataService.Start();

            // Get the main window from DI and show it
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            // Restore previously saved widget configuration
            _ = mainWindow.GetViewModel().LoadAndRestoreConfiguration();

            // Check for updates in background (non-blocking)
            _ = CheckForUpdatesAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to start application");
            ShowErrorDialog("Startup Error", ex);
            Shutdown(1);
        }

        base.OnStartup(e);
    }

    private void SetupExceptionHandling()
    {
        DispatcherUnhandledException += (s, e) =>
        {
            Log.Error(e.Exception, "Unhandled UI exception");
            ShowErrorDialog("Unexpected Error", e.Exception);
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            Dispatcher.Invoke(() => ShowErrorDialog("Background Task Error", e.Exception));
            e.SetObserved();
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.Fatal(ex, "Fatal unhandled exception");
                Dispatcher.Invoke(() => ShowErrorDialog("Fatal Error", ex));
            }
        };
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var mgr = new UpdateManager("https://github.com/Race-Overlay/RaceOverlay/releases");
            if (!mgr.IsInstalled)
            {
                Log.Information("App is not installed via Velopack, skipping update check");
                return;
            }

            var updateInfo = await mgr.CheckForUpdatesAsync();
            if (updateInfo != null)
            {
                Log.Information("Update available: {Version}", updateInfo.TargetFullRelease.Version);
                await mgr.DownloadUpdatesAsync(updateInfo);
                var result = System.Windows.MessageBox.Show(
                    $"Version {updateInfo.TargetFullRelease.Version} is available. Restart to update?",
                    "Update Available",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    mgr.ApplyUpdatesAndRestart(updateInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Update check failed");
        }
    }

    private static void ShowErrorDialog(string title, Exception ex)
    {
        var details = $"{ex.Message}\n\n{ex}";
        var dialog = new ErrorDialog(title, details);
        dialog.ShowDialog();
    }

    /// <summary>
    /// Called when the application exits.
    /// Disposes of the host and cleans up resources.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("RaceOverlay shutting down");
        _dataService?.Stop();
        _host?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    /// <summary>
    /// Configures all services for dependency injection.
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // iRacing telemetry service (singleton — shared across all widgets)
        services.AddSingleton<IRacingDataService>();
        services.AddSingleton<ILiveTelemetryService>(sp => sp.GetRequiredService<IRacingDataService>());
        services.AddSingleton<IGameProvider, IRacingProvider>();

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

            // Register Lap Timer Widget
            registry.RegisterWidget(new WidgetMetadata
            {
                WidgetId = "lap-timer",
                DisplayName = "Lap Timer",
                Description = "Displays current, last, and best lap times with delta comparisons.",
                WidgetType = typeof(LapTimerWidget),
                ConfigurationType = typeof(LapTimerConfig),
                Version = "1.0.0",
                Author = "RaceOverlay Team"
            });

            // Register Track Map Widget
            registry.RegisterWidget(new WidgetMetadata
            {
                WidgetId = "track-map",
                DisplayName = "Track Map",
                Description = "Minimap showing the track outline with colored dots for each car's position.",
                WidgetType = typeof(TrackMapWidget),
                ConfigurationType = typeof(TrackMapConfig),
                Version = "1.0.0",
                Author = "RaceOverlay Team"
            });

            // Register Weather Widget
            registry.RegisterWidget(new WidgetMetadata
            {
                WidgetId = "weather",
                DisplayName = "Weather",
                Description = "Displays track/air temperature, weather conditions, humidity, wind, and rain forecast.",
                WidgetType = typeof(WeatherWidget),
                ConfigurationType = typeof(WeatherConfig),
                Version = "1.0.0",
                Author = "RaceOverlay Team"
            });

            // Register Radar Widget
            registry.RegisterWidget(new WidgetMetadata
            {
                WidgetId = "radar",
                DisplayName = "Radar",
                Description = "Top-down proximity radar showing cars around you as rectangles.",
                WidgetType = typeof(RadarWidget),
                ConfigurationType = typeof(RadarConfig),
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

        // Lap Timer Widget
        services.AddTransient<LapTimerWidget>();
        services.AddTransient<LapTimerViewModel>();
        services.AddTransient<LapTimerView>();

        // Track Map Widget
        services.AddTransient<TrackMapWidget>();
        services.AddTransient<TrackMapViewModel>();
        services.AddTransient<TrackMapView>();

        // Weather Widget
        services.AddTransient<WeatherWidget>();
        services.AddTransient<WeatherViewModel>();
        services.AddTransient<WeatherView>();

        // Radar Widget
        services.AddTransient<RadarWidget>();
        services.AddTransient<RadarViewModel>();
        services.AddTransient<RadarView>();

        // Widget view factories
        services.AddSingleton<IWidgetViewFactory, RelativeOverlayViewFactory>();
        services.AddSingleton<IWidgetViewFactory, FuelCalculatorViewFactory>();
        services.AddSingleton<IWidgetViewFactory, InputsViewFactory>();
        services.AddSingleton<IWidgetViewFactory, InputTraceViewFactory>();
        services.AddSingleton<IWidgetViewFactory, StandingsViewFactory>();
        services.AddSingleton<IWidgetViewFactory, LapTimerViewFactory>();
        services.AddSingleton<IWidgetViewFactory, TrackMapViewFactory>();
        services.AddSingleton<IWidgetViewFactory, WeatherViewFactory>();
        services.AddSingleton<IWidgetViewFactory, RadarViewFactory>();
        services.AddSingleton<WidgetViewFactoryRegistry>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    }
}
