using System.Windows;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.Engine.Factories;

public class RelativeOverlayViewFactory : IWidgetViewFactory
{
    public string WidgetId => "relative-overlay";

    public FrameworkElement CreateView() => new RelativeOverlayView();

    public object CreateViewModel(IWidget widget)
    {
        var vm = new RelativeOverlayViewModel();
        var w = (RelativeOverlay)widget;
        if (w.Configuration is IRelativeOverlayConfig config)
            vm.ApplyConfiguration(config);
        vm.LoadRelativeDrivers(w.GetRelativeDrivers());
        return vm;
    }

    public void ApplyConfiguration(object viewModel, IWidgetConfiguration config)
    {
        if (viewModel is RelativeOverlayViewModel vm && config is IRelativeOverlayConfig c)
            vm.ApplyConfiguration(c);
    }

    public void RefreshData(object viewModel, IWidget widget)
    {
        if (viewModel is RelativeOverlayViewModel vm && widget is RelativeOverlay w)
            vm.RefreshDrivers(w.GetRelativeDrivers());
    }

    public Action Subscribe(object viewModel, IWidget widget, Action<Action> dispatcherInvoke)
    {
        var vm = (RelativeOverlayViewModel)viewModel;
        var w = (RelativeOverlay)widget;

        void OnDataUpdated() => dispatcherInvoke(() => vm.RefreshDrivers(w.GetRelativeDrivers()));

        w.DataUpdated += OnDataUpdated;
        return () => w.DataUpdated -= OnDataUpdated;
    }
}
