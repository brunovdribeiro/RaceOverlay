using System.IO;
using System.Text.Json;
using RaceOverlay.Core.Services;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.Services;

public class TrackOutlineProvider
{
    private static readonly string CacheDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RaceOverlay", "tracks");

    // Fallback D-shape outline used while recording or when no cache exists
    private static readonly (double X, double Y)[] FallbackOutline = new[]
    {
        (0.30, 0.05), (0.35, 0.04), (0.40, 0.03), (0.45, 0.03),
        (0.50, 0.03), (0.55, 0.03), (0.60, 0.04), (0.65, 0.05),
        (0.70, 0.07), (0.75, 0.10), (0.79, 0.14), (0.82, 0.18),
        (0.85, 0.23), (0.87, 0.28), (0.88, 0.33), (0.89, 0.38),
        (0.89, 0.43), (0.89, 0.48), (0.88, 0.53), (0.87, 0.58),
        (0.85, 0.63), (0.82, 0.68), (0.79, 0.72), (0.75, 0.76),
        (0.70, 0.80), (0.65, 0.83), (0.60, 0.85), (0.55, 0.87),
        (0.50, 0.88), (0.45, 0.88), (0.40, 0.87), (0.35, 0.85),
        (0.30, 0.83), (0.25, 0.80), (0.21, 0.76), (0.18, 0.72),
        (0.15, 0.68), (0.13, 0.63), (0.12, 0.58), (0.11, 0.53),
        (0.11, 0.48), (0.11, 0.43), (0.12, 0.38), (0.13, 0.33),
        (0.15, 0.28), (0.18, 0.23), (0.21, 0.18), (0.25, 0.14),
        (0.30, 0.10), (0.30, 0.05),
    };

    private const int TargetPointCount = 200;

    private (double X, double Y)[] _currentOutline = FallbackOutline;
    private bool _isRecording;
    private bool _hasRecordedOutline;
    private double _posX;
    private double _posY;
    private float _lastLapDistPct;
    private readonly List<(double LapDistPct, double X, double Y)> _samples = new();
    private int _trackId;
    private string _trackName = string.Empty;

    public (double X, double Y)[] CurrentOutline => _currentOutline;
    public bool IsRecording => _isRecording;

    public event Action? OutlineReady;

    public void Initialize(ILiveTelemetryService telemetryService)
    {
        _trackId = telemetryService.TrackId;
        _trackName = telemetryService.TrackName ?? "Unknown";

        if (_trackId <= 0)
        {
            _currentOutline = FallbackOutline;
            return;
        }

        // Try loading from cache
        var cached = LoadFromCache(_trackId);
        if (cached != null)
        {
            _currentOutline = cached;
            _hasRecordedOutline = true;
            return;
        }

        // Start recording
        _isRecording = true;
        _hasRecordedOutline = false;
        _posX = 0;
        _posY = 0;
        _lastLapDistPct = -1;
        _samples.Clear();
        _currentOutline = FallbackOutline;
    }

    public void RecordSample(float speed, float yaw, float lapDistPct, double dt)
    {
        if (!_isRecording || _hasRecordedOutline) return;

        // Integrate position using dead-reckoning
        _posX += speed * Math.Sin(yaw) * dt;
        _posY += speed * Math.Cos(yaw) * dt;

        _samples.Add((lapDistPct, _posX, _posY));

        // Detect lap completion: LapDistPct wraps from >0.9 to <0.1
        if (_lastLapDistPct > 0.9f && lapDistPct < 0.1f && _samples.Count > 50)
        {
            CompleteLap();
        }

        _lastLapDistPct = lapDistPct;
    }

    private void CompleteLap()
    {
        _isRecording = false;
        _hasRecordedOutline = true;

        var outline = DownsampleAndNormalize(_samples, TargetPointCount);
        _currentOutline = outline;

        SaveToCache(_trackId, _trackName, outline);
        _samples.Clear();

        OutlineReady?.Invoke();
    }

    internal static (double X, double Y)[] DownsampleAndNormalize(
        List<(double LapDistPct, double X, double Y)> samples, int targetCount)
    {
        if (samples.Count == 0) return FallbackOutline;

        // Sort by LapDistPct
        var sorted = samples.OrderBy(s => s.LapDistPct).ToList();

        // Downsample to evenly-spaced points by LapDistPct
        var downsampled = new List<(double X, double Y)>();
        for (int i = 0; i < targetCount; i++)
        {
            double targetPct = (double)i / targetCount;

            // Find the two surrounding samples
            int idx = sorted.FindIndex(s => s.LapDistPct >= targetPct);
            if (idx < 0) idx = sorted.Count - 1;
            if (idx == 0)
            {
                downsampled.Add((sorted[0].X, sorted[0].Y));
                continue;
            }

            var before = sorted[idx - 1];
            var after = sorted[idx];

            // Linear interpolation
            double range = after.LapDistPct - before.LapDistPct;
            double t = range > 0 ? (targetPct - before.LapDistPct) / range : 0;
            double x = before.X + (after.X - before.X) * t;
            double y = before.Y + (after.Y - before.Y) * t;
            downsampled.Add((x, y));
        }

        // Normalize to 0-1 range
        double minX = downsampled.Min(p => p.X);
        double maxX = downsampled.Max(p => p.X);
        double minY = downsampled.Min(p => p.Y);
        double maxY = downsampled.Max(p => p.Y);

        double rangeX = maxX - minX;
        double rangeY = maxY - minY;

        // Use the larger range to maintain aspect ratio
        double maxRange = Math.Max(rangeX, rangeY);
        if (maxRange < 0.001) return FallbackOutline;

        // Center the smaller dimension
        double offsetX = (maxRange - rangeX) / 2;
        double offsetY = (maxRange - rangeY) / 2;

        const double padding = 0.05;
        double scale = 1.0 - 2 * padding;

        return downsampled.Select(p => (
            X: padding + ((p.X - minX + offsetX) / maxRange) * scale,
            Y: padding + ((p.Y - minY + offsetY) / maxRange) * scale
        )).ToArray();
    }

    private static (double X, double Y)[]? LoadFromCache(int trackId)
    {
        var path = GetCachePath(trackId);
        if (!File.Exists(path)) return null;

        try
        {
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<TrackOutlineData>(json);
            if (data?.Points == null || data.Points.Length < 10) return null;
            return data.TrackOutline;
        }
        catch
        {
            return null;
        }
    }

    private static void SaveToCache(int trackId, string trackName, (double X, double Y)[] outline)
    {
        try
        {
            Directory.CreateDirectory(CacheDirectory);
            var data = TrackOutlineData.FromOutline(trackId, trackName, outline);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetCachePath(trackId), json);
        }
        catch
        {
            // Silently handle cache write failures
        }
    }

    private static string GetCachePath(int trackId) =>
        Path.Combine(CacheDirectory, $"{trackId}.json");

    public void Reset()
    {
        _isRecording = false;
        _hasRecordedOutline = false;
        _samples.Clear();
        _currentOutline = FallbackOutline;
    }
}
