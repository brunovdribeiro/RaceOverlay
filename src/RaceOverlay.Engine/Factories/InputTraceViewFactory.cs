using System.Windows;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.Engine.Factories;

public class InputTraceViewFactory : IWidgetViewFactory
{
    public string WidgetId => "input-trace";

    public FrameworkElement CreateView() => new InputTraceView();

    public object CreateViewModel(IWidget widget)
    {
        var vm = new InputTraceViewModel();
        var w = (InputTraceWidget)widget;
        if (w.Configuration is IInputTraceConfig config)
            vm.ApplyConfiguration(config);
        vm.UpdateTrace(w.GetTraceHistory());
        return vm;
    }

    public void ApplyConfiguration(object viewModel, IWidgetConfiguration config)
    {
        if (viewModel is InputTraceViewModel vm && config is IInputTraceConfig c)
            vm.ApplyConfiguration(c);
    }

    public void RefreshData(object viewModel, IWidget widget)
    {
        if (viewModel is InputTraceViewModel vm && widget is InputTraceWidget w)
            vm.UpdateTrace(w.GetTraceHistory());
    }

    public Action Subscribe(object viewModel, IWidget widget, Action<Action> dispatcherInvoke)
    {
        var vm = (InputTraceViewModel)viewModel;
        var w = (InputTraceWidget)widget;

        void OnDataUpdated() => dispatcherInvoke(() => vm.UpdateTrace(w.GetTraceHistory()));

        w.DataUpdated += OnDataUpdated;
        return () => w.DataUpdated -= OnDataUpdated;
    }
}
