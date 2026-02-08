using System.Globalization;
using System.Windows.Data;

namespace RaceOverlay.Engine.Converters;

/// <summary>
/// Converts an int positions-gained value to a formatted string: "+2", "-1", "0".
/// </summary>
public class PositionsGainedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int gained)
        {
            if (gained > 0) return $"+{gained}";
            if (gained < 0) return gained.ToString();
            return "0";
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
