using CZGL.SystemInfo;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Assistant.Utils.Converters;

public class LongSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long size)
        {
            var si = SizeInfo.Get(size);
            return $"{si.Size} {si.SizeType}";
        }

        return $"N/A";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}