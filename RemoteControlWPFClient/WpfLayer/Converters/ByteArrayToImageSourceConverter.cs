using System;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RemoteControlWPFClient.WpfLayer.Converters;

public class ByteArrayToImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte[] bytes)
        {
            return LoadImage(bytes);
        }

        return default;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    
    private BitmapImage LoadImage(byte[] bytes)
    {
        try
        {
            if (bytes == null || bytes.Length == 0) return null;
            var image = new BitmapImage();
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = ms;
                image.EndInit();
            }

            image.Freeze();
            return image;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return default;
        }
    }
}