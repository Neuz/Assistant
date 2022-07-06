using System.Collections.Generic;
using System.IO;
using CZGL.SystemInfo;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Assistant.ViewModel;

public class SystemInfoViewModel : ObservableObject
{
    public SystemInfoViewModel()
    {
    }
    
    public DiskInfo[] DiskInfos { get; } = DiskInfo.GetDisks();

    public Dictionary<string, object> SystemPlatformDict { get; }= new()
    {
        {"操作系统", SystemPlatformInfo.OSDescription},
        {"操作系统版本", SystemPlatformInfo.OSVersion},
        {"操作系统类型", SystemPlatformInfo.OSPlatformID},
        {"平台架构", SystemPlatformInfo.OSArchitecture},
        {"计算机名称	", SystemPlatformInfo.MachineName},
        {"当前用户名称", SystemPlatformInfo.UserName},
        {"用户网络域名称", SystemPlatformInfo.UserDomainName},
        {"系统目录", SystemPlatformInfo.SystemDirectory},
        {"框架平台", SystemPlatformInfo.FrameworkDescription},
        {"框架版本", SystemPlatformInfo.FrameworkVersion},
        {"CPU核心数", SystemPlatformInfo.ProcessorCount},
        {"CPU架构", SystemPlatformInfo.ProcessArchitecture},
    };
    
}