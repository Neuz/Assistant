using System;
using System.Globalization;
using System.Windows.Data;

namespace Assistant.Utils.Converters;

public class BoolStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var trueValue  = $"{parameter}".IndexOf('|') < 0 ? "True" : $"{parameter}".Split('|')[0];
        var falseValue = $"{parameter}".IndexOf('|') < 0 ? "False" : $"{parameter}".Split('|')[1];
        return System.Convert.ToBoolean(value) ? trueValue : falseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}