using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace RemoteControlWPFClient.WpfLayer.Converters;

public class ByteArrayToMemoryStreamConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte[] bytes)
        {
            return new MemoryStream(bytes);
        }

        return default;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}