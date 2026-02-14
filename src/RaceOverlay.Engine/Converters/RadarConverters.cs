using System.Globalization;
using System.Windows.Data;

namespace RaceOverlay.Engine.Converters;

public class RadarXConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not double latOffset || values[1] is not double canvasWidth)
            return 0.0;

        // latOffset is in meters. 1 meter ~ 10 pixels?
        // Center of canvas is canvasWidth / 2
        double pixelsPerMeter = 10.0;
        return (canvasWidth / 2) + (latOffset * pixelsPerMeter) - 10; // -10 to center the 20px wide rectangle
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RadarYConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3 || values[0] is not double longOffset || values[1] is not double rangeMeters || values[2] is not double canvasHeight)
            return 0.0;

        // Player is at 150 (center of 300 height) or roughly canvasHeight / 2
        // If car is ahead, longOffset is positive. In WPF Y goes down, so we subtract longOffset.
        double pixelsPerMeter = (canvasHeight / 2) / rangeMeters;
        return (canvasHeight / 2) - (longOffset * pixelsPerMeter) - 17.5; // -17.5 to center the 35px high rectangle
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
