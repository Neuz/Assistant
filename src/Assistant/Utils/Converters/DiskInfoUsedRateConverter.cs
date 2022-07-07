using CZGL.SystemInfo;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Assistant.Utils.Converters;

public class DiskInfoUsedRateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DiskInfo diskInfo)
        {
            var rate = System.Convert.ToDouble(diskInfo.UsedSize) / System.Convert.ToDouble(diskInfo.TotalSize) * 100;
            return Math.Round(rate, 2);
        }

        return 0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}