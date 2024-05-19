using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using DevExpress.Mvvm.Native;

namespace RemoteControlWPFClient.WpfLayer.Converters;

public class CollectionToPathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<object>) return value; 
        IEnumerable<object> list = (value as IEnumerable<object>)!;
        return string.Join("\\", list);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
        if (value is not string) return default;

        string[] values = (value as string)!.Split('\\');
        return values.ToObservableCollection();
    }
}