using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Assistant.Utils.Converters;

public class BoolColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return System.Convert.ToBoolean(value) ? new SolidColorBrush(Color.FromRgb(152, 195, 121)) : new SolidColorBrush(Color.FromRgb(224, 108, 117));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}