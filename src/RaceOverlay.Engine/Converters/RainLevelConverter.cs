using System.Globalization;
using System.Windows.Data;

namespace RaceOverlay.Engine.Converters;

public class RainLevelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percent && parameter is string zone)
        {
            return zone switch
            {
                "red" => percent > 60,
                "yellow" => percent > 30 && percent <= 60,
                _ => false
            };
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
