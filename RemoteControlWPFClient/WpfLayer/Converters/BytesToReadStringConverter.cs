using System;
using System.Globalization;
using System.Windows.Data;

namespace RemoteControlWPFClient.WpfLayer.Converters;

public class BytesToReadStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not long) return value;

        long length = (long)value!;
        double res = length;
        int degreeCount = 0;
        while (length.ToString().Length > 3)
        {
            degreeCount++;
            if (degreeCount > 5)
            {
                break;
            }

            length /= 1024;
            res /= 1024.0;
        }

        return res.ToString("F2") + " " + degreeCount switch
        {
            0 => "Б",
            1 => "КБ",
            2 => "МБ",
            3 => "ГБ",
            4 => "ТБ",
            5 => "ПБ",
            _ => "Ту мач бро"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}