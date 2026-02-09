using CommunityToolkit.Mvvm.ComponentModel;

namespace RaceOverlay.App.Models;

public partial class ActiveWidgetCard : ObservableObject
{
    public string WidgetId { get; }
    public string DisplayName { get; }
    public string IconKey { get; }

    [ObservableProperty]
    private bool isSelected;

    public ActiveWidgetCard(string widgetId, string displayName, string iconKey)
    {
        WidgetId = widgetId;
        DisplayName = displayName;
        IconKey = iconKey;
    }
}
