using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RaceOverlay.Engine.Converters;

/// <summary>
/// Converts a hex color string (e.g. "#D946EF") to a SolidColorBrush for WPF binding.
/// </summary>
public class StringToSolidColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString && !string.IsNullOrEmpty(colorString))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                return new SolidColorBrush(color);
            }
            catch
            {
                // Fall through to default
            }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
