using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaceOverlay.App.ViewModels;
using RaceOverlay.Engine.Widgets;

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
        services.AddSingleton<IWidgetRegistry, WidgetRegistry>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    }}