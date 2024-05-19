using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace RemoteControlWPFClient.WpfLayer.Converters;

public class CollectionLengthGreaterThenConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<object> && parameter is not string) return false; 
        IEnumerable<object> list = (value as IEnumerable<object>)!;
        int comparisonLength;
        if (!int.TryParse(parameter as string, out comparisonLength))
        {
            comparisonLength = 0;
        }        

        return list.Count() > comparisonLength;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}