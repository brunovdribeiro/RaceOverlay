using System.Windows;
using RaceOverlay.Core.Widgets;

namespace RaceOverlay.Core.Widgets;

/// <summary>
/// Factory that knows how to create, wire, and manage the view and viewmodel
/// for a specific widget type. Eliminates type-checking if/else chains in
/// the hosting infrastructure.
/// </summary>
public interface IWidgetViewFactory
{
    /// <summary>
    /// The widget ID this factory handles (e.g. "standings", "fuel-calculator").
    /// </summary>
    string WidgetId { get; }

    /// <summary>
    /// Creates the WPF view for this widget.
    /// </summary>
    FrameworkElement CreateView();

    /// <summary>
    /// Creates and returns the viewmodel, with configuration already applied
    /// and initial data loaded from the widget.
    /// </summary>
    object CreateViewModel(IWidget widget);

    /// <summary>
    /// Applies a configuration object to an existing viewmodel.
    /// Called when the user changes settings at runtime.
    /// </summary>
    void ApplyConfiguration(object viewModel, IWidgetConfiguration config);

    /// <summary>
    /// Refreshes data on the viewmodel from the widget (one-shot pull).
    /// Called when exiting drag mode.
    /// </summary>
    void RefreshData(object viewModel, IWidget widget);

    /// <summary>
    /// Subscribes the viewmodel to the widget's data-update events.
    /// Returns an Action that, when invoked, unsubscribes.
    /// </summary>
    Action Subscribe(object viewModel, IWidget widget, Action<Action> dispatcherInvoke);
}
