using System.Windows;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.Engine.Factories;

public class LapTimerViewFactory : IWidgetViewFactory
{
    public string WidgetId => "lap-timer";

    public FrameworkElement CreateView() => new LapTimerView();

    public object CreateViewModel(IWidget widget)
    {
        var vm = new LapTimerViewModel();
        var w = (LapTimerWidget)widget;
        if (w.Configuration is ILapTimerConfig config)
            vm.ApplyConfiguration(config);
        vm.UpdateLapData(w.GetLapTimerData());
        return vm;
    }

    public void ApplyConfiguration(object viewModel, IWidgetConfiguration config)
    {
        if (viewModel is LapTimerViewModel vm && config is ILapTimerConfig c)
            vm.ApplyConfiguration(c);
    }

    public void RefreshData(object viewModel, IWidget widget)
    {
        if (viewModel is LapTimerViewModel vm && widget is LapTimerWidget w)
            vm.UpdateLapData(w.GetLapTimerData());
    }

    public Action Subscribe(object viewModel, IWidget widget, Action<Action> dispatcherInvoke)
    {
        var vm = (LapTimerViewModel)viewModel;
        var w = (LapTimerWidget)widget;

        void OnDataUpdated() => dispatcherInvoke(() => vm.UpdateLapData(w.GetLapTimerData()));

        w.DataUpdated += OnDataUpdated;
        return () => w.DataUpdated -= OnDataUpdated;
    }
}
