using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Assistant.Utils.Converters;

public class MacAddressConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            var regex   = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
            var replace = "$1-$2-$3-$4-$5-$6";
            return Regex.Replace(str, regex, replace);
        }

        return $"{value}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}