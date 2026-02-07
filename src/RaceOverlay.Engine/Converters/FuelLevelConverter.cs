using System.Globalization;
using System.Windows.Data;

namespace RaceOverlay.Engine.Converters;

public class FuelLevelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percent && parameter is string zone)
        {
            return zone switch
            {
                "red" => percent < 15,
                "yellow" => percent >= 15 && percent < 30,
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
