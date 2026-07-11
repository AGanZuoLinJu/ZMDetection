using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ZMDetection.Converters;

public sealed class ConnectionStatusToBrushConverter : IValueConverter
{
    private static readonly Brush ConnectedBrush = new SolidColorBrush(Color.FromRgb(0x17, 0xA3, 0x4A));
    private static readonly Brush DisconnectedBrush = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool result = (bool)value!;
        if (result)
        {
            return ConnectedBrush;
        }
        else
        {
            return DisconnectedBrush;
        }
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
