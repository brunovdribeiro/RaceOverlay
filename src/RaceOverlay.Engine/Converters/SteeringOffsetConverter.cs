using System.Globalization;
using System.Windows.Data;

namespace RaceOverlay.Engine.Converters;

public class SteeringOffsetConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is double steering && values[1] is double containerWidth)
        {
            double clamped = Math.Max(-1, Math.Min(1, steering));
            // Map -1..+1 to -halfWidth..+halfWidth
            return clamped * (containerWidth / 2.0 - 8); // 8 = half of dot width
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [];
    }
}
