using Assistant.Model.BaseServices;
using Assistant.Model.OperationsTools;
using System;
using System.Globalization;
using System.Windows.Data;
using static Assistant.Model.OperationsTools.WinSvc;

namespace Assistant.Converters
{
    internal class WinSvcToColorConverter : IValueConverter
    {
        // todo
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                null => "Red",
                WinSvc v => v.Status == RunningStatus.Running
                                ? "GreenYellow" // 运行
                                : "Red"         // 停止及其他
                ,
                _ => "Grey"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
