using System.Globalization;
using System.Windows.Data;

namespace RaceOverlay.Engine.Converters;

public class FuelBarWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is double percent && values[1] is double containerWidth)
        {
            double clampedPercent = Math.Max(0, Math.Min(100, percent));
            return containerWidth * clampedPercent / 100.0;
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [];
    }
}
