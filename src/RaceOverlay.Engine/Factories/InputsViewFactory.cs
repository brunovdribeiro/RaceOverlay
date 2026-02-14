using System.Windows;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.Engine.Factories;

public class InputsViewFactory : IWidgetViewFactory
{
    public string WidgetId => "inputs";

    public FrameworkElement CreateView() => new InputsView();

    public object CreateViewModel(IWidget widget)
    {
        var vm = new InputsViewModel();
        var w = (InputsWidget)widget;
        if (w.Configuration is IInputsConfig config)
            vm.ApplyConfiguration(config);
        vm.UpdateInputsData(w.GetInputsData());
        return vm;
    }

    public void ApplyConfiguration(object viewModel, IWidgetConfiguration config)
    {
        if (viewModel is InputsViewModel vm && config is IInputsConfig c)
            vm.ApplyConfiguration(c);
    }

    public void RefreshData(object viewModel, IWidget widget)
    {
        if (viewModel is InputsViewModel vm && widget is InputsWidget w)
            vm.UpdateInputsData(w.GetInputsData());
    }

    public Action Subscribe(object viewModel, IWidget widget, Action<Action> dispatcherInvoke)
    {
        var vm = (InputsViewModel)viewModel;
        var w = (InputsWidget)widget;

        void OnDataUpdated() => dispatcherInvoke(() => vm.UpdateInputsData(w.GetInputsData()));

        w.DataUpdated += OnDataUpdated;
        return () => w.DataUpdated -= OnDataUpdated;
    }
}
