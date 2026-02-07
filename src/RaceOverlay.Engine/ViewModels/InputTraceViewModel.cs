using CommunityToolkit.Mvvm.ComponentModel;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.ViewModels;

public partial class InputTraceViewModel : ObservableObject
{
    [ObservableProperty]
    private string throttleColor = "#22C55E";

    [ObservableProperty]
    private string brakeColor = "#EF4444";

    [ObservableProperty]
    private int historySeconds = 10;

    public IReadOnlyList<InputTracePoint> TraceHistory { get; private set; } = Array.Empty<InputTracePoint>();

    public event Action? TraceUpdated;

    public void ApplyConfiguration(IInputTraceConfig config)
    {
        ThrottleColor = config.ThrottleColor;
        BrakeColor = config.BrakeColor;
        HistorySeconds = config.HistorySeconds;
    }

    public void UpdateTrace(IReadOnlyList<InputTracePoint> history)
    {
        TraceHistory = history;
        TraceUpdated?.Invoke();
    }
}
