using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Assistant.Model.ServiceManager;

namespace Assistant.Utils.Converters;

public class EnumColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 运行状态
        if ((Type) parameter == RunningStatus.Running.GetType())
        {
            return (RunningStatus)value switch
            {
                RunningStatus.Running => new SolidColorBrush(Colors.Green),
                RunningStatus.Stopped => new SolidColorBrush(Colors.Red),
                RunningStatus.UnKnown => new SolidColorBrush(Colors.Orange),
                _ => new SolidColorBrush(Colors.White)
            };
        }

        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}