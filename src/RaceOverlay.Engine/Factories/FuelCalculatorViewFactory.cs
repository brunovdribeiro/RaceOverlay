using System.Windows;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.Engine.Factories;

public class FuelCalculatorViewFactory : IWidgetViewFactory
{
    public string WidgetId => "fuel-calculator";

    public FrameworkElement CreateView() => new FuelCalculatorView();

    public object CreateViewModel(IWidget widget)
    {
        var vm = new FuelCalculatorViewModel();
        var w = (FuelCalculator)widget;
        if (w.Configuration is IFuelCalculatorConfig config)
            vm.ApplyConfiguration(config);
        vm.UpdateFuelData(w.GetFuelData());
        return vm;
    }

    public void ApplyConfiguration(object viewModel, IWidgetConfiguration config)
    {
        if (viewModel is FuelCalculatorViewModel vm && config is IFuelCalculatorConfig c)
            vm.ApplyConfiguration(c);
    }

    public void RefreshData(object viewModel, IWidget widget)
    {
        if (viewModel is FuelCalculatorViewModel vm && widget is FuelCalculator w)
            vm.UpdateFuelData(w.GetFuelData());
    }

    public Action Subscribe(object viewModel, IWidget widget, Action<Action> dispatcherInvoke)
    {
        var vm = (FuelCalculatorViewModel)viewModel;
        var w = (FuelCalculator)widget;

        void OnDataUpdated() => dispatcherInvoke(() => vm.UpdateFuelData(w.GetFuelData()));

        w.DataUpdated += OnDataUpdated;
        return () => w.DataUpdated -= OnDataUpdated;
    }
}
