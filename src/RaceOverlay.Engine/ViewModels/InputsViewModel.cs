using CommunityToolkit.Mvvm.ComponentModel;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.Models;

namespace RaceOverlay.Engine.ViewModels;

public partial class InputsViewModel : ObservableObject
{
    [ObservableProperty]
    private double throttle;

    [ObservableProperty]
    private double brake;

    [ObservableProperty]
    private double steering;

    [ObservableProperty]
    private double clutch;

    [ObservableProperty]
    private int gear;

    [ObservableProperty]
    private double speed;

    [ObservableProperty]
    private string throttleColor = "#22C55E";

    [ObservableProperty]
    private string brakeColor = "#EF4444";

    [ObservableProperty]
    private string clutchColor = "#3B82F6";

    [ObservableProperty]
    private bool showClutch = false;

    public void ApplyConfiguration(IInputsConfig config)
    {
        ThrottleColor = config.ThrottleColor;
        BrakeColor = config.BrakeColor;
        ClutchColor = config.ClutchColor;
        ShowClutch = config.ShowClutch;
    }

    public void UpdateInputsData(InputsData data)
    {
        Throttle = data.Throttle;
        Brake = data.Brake;
        Steering = data.Steering;
        Clutch = data.Clutch;
        Gear = data.Gear;
        Speed = data.Speed;
    }
}
