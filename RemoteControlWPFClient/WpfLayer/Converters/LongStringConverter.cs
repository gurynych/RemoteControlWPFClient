using System;
using System.Globalization;
using System.Windows.Data;

namespace RemoteControlWPFClient.WpfLayer.Converters;

public class LongStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string && parameter is not string) return value;

        string str = (value as string)!;
        if (!int.TryParse(parameter as string, out int maxLength)) return value;
        if (str.Length <= maxLength) return value;
        
        string endStr = "...";
        str = str.Substring(0, maxLength - endStr.Length);
        return str + endStr;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}