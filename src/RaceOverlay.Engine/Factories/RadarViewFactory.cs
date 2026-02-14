using System.Windows;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.Engine.Factories;

public class RadarViewFactory : IWidgetViewFactory
{
    public string WidgetId => "radar";

    public FrameworkElement CreateView() => new RadarView();

    public object CreateViewModel(IWidget widget)
    {
        var vm = new RadarViewModel();
        if (widget is RadarWidget w)
        {
            if (w.Configuration is IRadarConfig config)
                vm.ApplyConfiguration(config);
            
            vm.RefreshCars(w.GetCars());
        }
        return vm;
    }

    public void ApplyConfiguration(object viewModel, IWidgetConfiguration config)
    {
        if (viewModel is RadarViewModel vm && config is IRadarConfig c)
            vm.ApplyConfiguration(c);
    }

    public void RefreshData(object viewModel, IWidget widget)
    {
        if (viewModel is RadarViewModel vm && widget is RadarWidget w)
            vm.RefreshCars(w.GetCars());
    }

    public Action Subscribe(object viewModel, IWidget widget, Action<Action> dispatcherInvoke)
    {
        var vm = (RadarViewModel)viewModel;
        var w = (RadarWidget)widget;

        void OnDataUpdated() => dispatcherInvoke(() => vm.RefreshCars(w.GetCars()));

        w.DataUpdated += OnDataUpdated;
        return () => w.DataUpdated -= OnDataUpdated;
    }
}
