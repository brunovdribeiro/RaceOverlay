namespace RaceOverlay.Core.Widgets;

public interface IFuelCalculatorConfig : IWidgetConfiguration
{
    string IWidgetConfiguration.ConfigurationType => "FuelCalculator";

    double FuelTankCapacity { get; set; }
    int UpdateIntervalMs { get; set; }
    bool UseMockData { get; set; }
}
