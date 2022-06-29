using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public class MySQLServiceModel : ServiceBaseModel
{
    public static MySQLServiceModel GetDefaultProfile()
    {
        var serviceDirectory = Path.Combine(Global.CurrentDir, "Services", "MySQL");
        return new MySQLServiceModel
        {
            DisplayName        = "MySQL",
            ServiceName        = "Neuz.MySQL",
            ServicePath        = Path.Combine(serviceDirectory, "bin", "mysqld.exe"),
            ServiceDescription = "Neuz.MySQL 数据服务",
            ServiceDirectory   = serviceDirectory,
            LogDirectory       = serviceDirectory,
            Installed          = false,
            RunningStatus      = RunningStatus.UnKnown
        };
    }
}
