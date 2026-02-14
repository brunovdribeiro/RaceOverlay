using System.Windows;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.Engine.Factories;

public class StandingsViewFactory : IWidgetViewFactory
{
    public string WidgetId => "standings";

    public FrameworkElement CreateView() => new StandingsView();

    public object CreateViewModel(IWidget widget)
    {
        var vm = new StandingsViewModel();
        var w = (StandingsWidget)widget;
        if (w.Configuration is IStandingsConfig config)
            vm.ApplyConfiguration(config);
        vm.UpdateStandings(w.GetStandings(), w.CurrentLap, w.TotalLaps);
        return vm;
    }

    public void ApplyConfiguration(object viewModel, IWidgetConfiguration config)
    {
        if (viewModel is StandingsViewModel vm && config is IStandingsConfig c)
            vm.ApplyConfiguration(c);
    }

    public void RefreshData(object viewModel, IWidget widget)
    {
        if (viewModel is StandingsViewModel vm && widget is StandingsWidget w)
            vm.UpdateStandings(w.GetStandings(), w.CurrentLap, w.TotalLaps);
    }

    public Action Subscribe(object viewModel, IWidget widget, Action<Action> dispatcherInvoke)
    {
        var vm = (StandingsViewModel)viewModel;
        var w = (StandingsWidget)widget;

        void OnDataUpdated() => dispatcherInvoke(() => vm.UpdateStandings(w.GetStandings(), w.CurrentLap, w.TotalLaps));

        w.DataUpdated += OnDataUpdated;
        return () => w.DataUpdated -= OnDataUpdated;
    }
}
