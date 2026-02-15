using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Widgets;
using Serilog;

namespace RaceOverlay.App.Services;

public class AppState
{
    [JsonPropertyName("activeWidgets")]
    public List<string> ActiveWidgets { get; set; } = new();

    [JsonPropertyName("widgetConfigs")]
    public Dictionary<string, JsonElement> WidgetConfigs { get; set; } = new();
}

public class ConfigurationPersistenceService
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RaceOverlay");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    public void Save(Dictionary<string, IWidgetConfiguration> configs, List<string> activeWidgetIds)
    {
        try
        {
            var state = new AppState
            {
                ActiveWidgets = activeWidgetIds
            };

            foreach (var (widgetId, config) in configs)
            {
                var json = JsonSerializer.Serialize(config, config.GetType(), WriteOptions);
                state.WidgetConfigs[widgetId] = JsonDocument.Parse(json).RootElement.Clone();
            }

            Directory.CreateDirectory(ConfigDir);
            var jsonString = JsonSerializer.Serialize(state, WriteOptions);
            File.WriteAllText(ConfigPath, jsonString);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save configuration");
        }
    }

    public AppState? Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
                return null;

            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppState>(json);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load configuration");
            return null;
        }
    }

    public IWidgetConfiguration? DeserializeConfig(string widgetId, JsonElement element)
    {
        try
        {
            var json = element.GetRawText();
            return widgetId switch
            {
                "relative-overlay" => JsonSerializer.Deserialize<RelativeOverlayConfig>(json),
                "fuel-calculator" => JsonSerializer.Deserialize<FuelCalculatorConfig>(json),
                "inputs" => JsonSerializer.Deserialize<InputsConfig>(json),
                "input-trace" => JsonSerializer.Deserialize<InputTraceConfig>(json),
                "standings" => JsonSerializer.Deserialize<StandingsConfig>(json),
                "lap-timer" => JsonSerializer.Deserialize<LapTimerConfig>(json),
                "track-map" => JsonSerializer.Deserialize<TrackMapConfig>(json),
                "weather" => JsonSerializer.Deserialize<WeatherConfig>(json),
                "radar" => JsonSerializer.Deserialize<RadarConfig>(json),
                _ => null
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to deserialize config for {WidgetId}", widgetId);
            return null;
        }
    }
}
