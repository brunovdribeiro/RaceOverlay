using System.Globalization;
using System.Windows.Data;

namespace RaceOverlay.Engine.Converters;

public class InputBarHeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is double fraction && values[1] is double containerHeight)
        {
            double clamped = Math.Max(0, Math.Min(1, fraction));
            return containerHeight * clamped;
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [];
    }
}
