using Microsoft.Extensions.DependencyInjection;
using RaceOverlay.Core.Widgets;

namespace RaceOverlay.Engine.Widgets;

/// <summary>
/// Default implementation of IWidgetRegistry that manages widget registration and instantiation.
/// </summary>
public class WidgetRegistry : IWidgetRegistry
{
    private readonly Dictionary<string, WidgetMetadata> _widgets = new();

    /// <summary>
    /// Registers a widget type in the registry.
    /// </summary>
    /// <param name="metadata">Metadata describing the widget type.</param>
    /// <exception cref="ArgumentException">Thrown if a widget with the same ID is already registered.</exception>
    public void RegisterWidget(WidgetMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        if (_widgets.ContainsKey(metadata.WidgetId))
        {
            throw new ArgumentException($"Widget with ID '{metadata.WidgetId}' is already registered.", nameof(metadata));
        }

        _widgets[metadata.WidgetId] = metadata;
    }

    /// <summary>
    /// Gets metadata for all registered widgets.
    /// </summary>
    public IReadOnlyList<WidgetMetadata> GetRegisteredWidgets()
    {
        return _widgets.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets metadata for a specific widget by ID.
    /// </summary>
    public WidgetMetadata? GetWidgetMetadata(string widgetId)
    {
        ArgumentException.ThrowIfNullOrEmpty(widgetId, nameof(widgetId));

        _widgets.TryGetValue(widgetId, out var metadata);
        return metadata;
    }

    /// <summary>
    /// Creates an instance of a widget by its ID using the provided service provider.
    /// </summary>
    /// <param name="widgetId">The ID of the widget to create.</param>
    /// <param name="serviceProvider">Service provider for dependency injection.</param>
    /// <returns>A new widget instance, or null if the widget ID is not found.</returns>
    public IWidget? CreateWidget(string widgetId, IServiceProvider serviceProvider)
    {
        ArgumentException.ThrowIfNullOrEmpty(widgetId, nameof(widgetId));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

        var metadata = GetWidgetMetadata(widgetId);
        if (metadata == null)
        {
            return null;
        }

        try
        {
            var instance = ActivatorUtilities.CreateInstance(serviceProvider, metadata.WidgetType);
            return instance as IWidget;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create instance of widget '{widgetId}'.",
                ex);
        }
    }

    /// <summary>
    /// Removes a widget from the registry.
    /// </summary>
    /// <param name="widgetId">The ID of the widget to remove.</param>
    /// <returns>True if the widget was removed; false if it was not found.</returns>
    public bool UnregisterWidget(string widgetId)
    {
        ArgumentException.ThrowIfNullOrEmpty(widgetId, nameof(widgetId));
        return _widgets.Remove(widgetId);
    }
}
