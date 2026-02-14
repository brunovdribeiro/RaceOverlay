using System.Windows;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.Engine.Factories;

public class TrackMapViewFactory : IWidgetViewFactory
{
    public string WidgetId => "track-map";

    public FrameworkElement CreateView() => new TrackMapView();

    public object CreateViewModel(IWidget widget)
    {
        var vm = new TrackMapViewModel();
        var w = (TrackMapWidget)widget;
        if (w.Configuration is ITrackMapConfig config)
            vm.ApplyConfiguration(config);
        vm.TrackOutline = w.GetTrackOutline();
        var data = w.GetTrackMapData();
        vm.UpdateMap(data.Drivers, data.CurrentLap, data.TotalLaps);
        return vm;
    }

    public void ApplyConfiguration(object viewModel, IWidgetConfiguration config)
    {
        if (viewModel is TrackMapViewModel vm && config is ITrackMapConfig c)
            vm.ApplyConfiguration(c);
    }

    public void RefreshData(object viewModel, IWidget widget)
    {
        if (viewModel is TrackMapViewModel vm && widget is TrackMapWidget w)
        {
            vm.TrackOutline = w.GetTrackOutline();
            var data = w.GetTrackMapData();
            vm.UpdateMap(data.Drivers, data.CurrentLap, data.TotalLaps);
        }
    }

    public Action Subscribe(object viewModel, IWidget widget, Action<Action> dispatcherInvoke)
    {
        var vm = (TrackMapViewModel)viewModel;
        var w = (TrackMapWidget)widget;

        void OnDataUpdated()
        {
            var data = w.GetTrackMapData();
            dispatcherInvoke(() => vm.UpdateMap(data.Drivers, data.CurrentLap, data.TotalLaps));
        }

        void OnOutlineChanged()
        {
            dispatcherInvoke(() =>
            {
                vm.TrackOutline = w.GetTrackOutline();
                vm.NotifyOutlineChanged();
            });
        }

        w.DataUpdated += OnDataUpdated;
        w.OutlineChanged += OnOutlineChanged;
        return () =>
        {
            w.DataUpdated -= OnDataUpdated;
            w.OutlineChanged -= OnOutlineChanged;
        };
    }
}
