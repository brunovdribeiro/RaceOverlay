using RaceOverlay.Core.Widgets;

namespace RaceOverlay.Engine.Factories;

/// <summary>
/// Holds all registered IWidgetViewFactory instances, indexed by widget ID.
/// </summary>
public class WidgetViewFactoryRegistry
{
    private readonly Dictionary<string, IWidgetViewFactory> _factories = new();

    public WidgetViewFactoryRegistry(IEnumerable<IWidgetViewFactory> factories)
    {
        foreach (var factory in factories)
            _factories[factory.WidgetId] = factory;
    }

    public IWidgetViewFactory? GetFactory(string widgetId)
    {
        _factories.TryGetValue(widgetId, out var factory);
        return factory;
    }
}
