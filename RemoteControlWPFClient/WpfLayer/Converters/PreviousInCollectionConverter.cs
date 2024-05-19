using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace RemoteControlWPFClient.WpfLayer.Converters;

public class PreviousInCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<object>) return value; 
        IEnumerable<object> list = (value as IEnumerable<object>)!;
        return list.SkipLast(1).LastOrDefault();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}