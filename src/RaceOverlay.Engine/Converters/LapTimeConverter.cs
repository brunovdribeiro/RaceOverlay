using System.Globalization;
using System.Windows.Data;

namespace RaceOverlay.Engine.Converters;

/// <summary>
/// Converts a double (seconds) to a formatted lap time string (m:ss.fff).
/// Example: 102.34 → "1:42.340"
/// </summary>
public class LapTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double seconds && seconds > 0)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalMinutes}:{ts.Seconds:D2}.{ts.Milliseconds:D3}";
        }
        return "0:00.000";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
