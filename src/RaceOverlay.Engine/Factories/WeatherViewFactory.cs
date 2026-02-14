using System.Windows;
using RaceOverlay.Core.Widgets;
using RaceOverlay.Engine.ViewModels;
using RaceOverlay.Engine.Views;
using RaceOverlay.Engine.Widgets;

namespace RaceOverlay.Engine.Factories;

public class WeatherViewFactory : IWidgetViewFactory
{
    public string WidgetId => "weather";

    public FrameworkElement CreateView() => new WeatherView();

    public object CreateViewModel(IWidget widget)
    {
        var vm = new WeatherViewModel();
        var w = (WeatherWidget)widget;
        if (w.Configuration is IWeatherConfig config)
            vm.ApplyConfiguration(config);
        vm.UpdateWeather(w.GetWeatherData());
        return vm;
    }

    public void ApplyConfiguration(object viewModel, IWidgetConfiguration config)
    {
        if (viewModel is WeatherViewModel vm && config is IWeatherConfig c)
            vm.ApplyConfiguration(c);
    }

    public void RefreshData(object viewModel, IWidget widget)
    {
        if (viewModel is WeatherViewModel vm && widget is WeatherWidget w)
            vm.UpdateWeather(w.GetWeatherData());
    }

    public Action Subscribe(object viewModel, IWidget widget, Action<Action> dispatcherInvoke)
    {
        var vm = (WeatherViewModel)viewModel;
        var w = (WeatherWidget)widget;

        void OnDataUpdated() => dispatcherInvoke(() => vm.UpdateWeather(w.GetWeatherData()));

        w.DataUpdated += OnDataUpdated;
        return () => w.DataUpdated -= OnDataUpdated;
    }
}
