using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Assistant.Utils.Converters;

public class EnumStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null 
                   ? DependencyProperty.UnsetValue 
                   : GetDescription((Enum)value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }

    private static string GetDescription(Enum en)
    {
        var type    = en.GetType();
        var memInfo = type.GetMember(en.ToString());
        if (memInfo is {Length: > 0})
        {
            var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs is {Length: > 0}) return ((DescriptionAttribute) attrs[0]).Description;
        }

        return en.ToString();
    }
}