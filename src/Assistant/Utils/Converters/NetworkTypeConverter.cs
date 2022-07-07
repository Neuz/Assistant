using CZGL.SystemInfo;
using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Assistant.Utils.Converters;

public class NetworkTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is NetworkInterfaceType networkType)
            if (networkType == NetworkInterfaceType.Wireless80211)
            {
                var rs = Application.Current.TryFindResource("Svg.Wifi") as Geometry;
                return rs == null ? "" : rs;
            }

        var defaultResult = Application.Current.TryFindResource("Svg.Apartment") as Geometry;
        return defaultResult == null ? "" : defaultResult;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}