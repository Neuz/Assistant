using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public class RedisServiceModel : ServiceBaseModel
{
    public RedisServiceModel()
    {
        var baseDir = Path.Combine(Global.CurrentDir, "Services", "Redis");
        DisplayName        = "Redis";
        Version            = "5.0.14.1";
        ServiceName        = "Neuz.Redis";
        BinPath            = Path.Combine(baseDir, "redis-server.exe");
        ServiceDescription = "Neuz.Redis 服务";
        ServiceDirectory   = baseDir;
        LogDirectory       = baseDir;
        ConfigFilePath     = Path.Combine(baseDir, "redis.conf");
        Installed          = false;
        RunningStatus      = RunningStatus.UnKnown;
    }
    
}