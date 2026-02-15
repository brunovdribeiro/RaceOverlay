using CommunityToolkit.Mvvm.ComponentModel;
using RaceOverlay.Core.Widgets;

namespace RaceOverlay.App.Models;

public partial class WidgetLibraryItem : ObservableObject
{
    public WidgetMetadata Metadata { get; }
    public string DisplayName => Metadata.DisplayName;
    public string WidgetId => Metadata.WidgetId;

    private readonly Action<WidgetLibraryItem, bool>? _toggleCallback;

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private bool isSupported = true;

    public WidgetLibraryItem(WidgetMetadata metadata, Action<WidgetLibraryItem, bool>? toggleCallback = null)
    {
        Metadata = metadata;
        _toggleCallback = toggleCallback;
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _toggleCallback?.Invoke(this, value);
    }
}
