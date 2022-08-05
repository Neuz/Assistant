using CommunityToolkit.Mvvm.ComponentModel;
using CZGL.SystemInfo;
using System;
using System.Collections.Generic;

namespace Assistant.ViewModel;

public class SystemInfoViewModel : ObservableObject
{
    public SystemInfoViewModel()
    {
        var ts = TimeSpan.FromMilliseconds(Environment.TickCount);
        SystemPlatformDict = new Dictionary<string, object>
        {
            {"操作系统", SystemPlatformInfo.OSDescription},
            {"操作系统版本", SystemPlatformInfo.OSVersion},
            {"操作系统类型", SystemPlatformInfo.OSPlatformID},
            {"平台架构", SystemPlatformInfo.OSArchitecture},
            {"计算机名称	", SystemPlatformInfo.MachineName},
            {"当前用户名称", SystemPlatformInfo.UserName},
            {"用户网络域名称", SystemPlatformInfo.UserDomainName},
            {"系统目录", SystemPlatformInfo.SystemDirectory},
            {"系统运行时间", $"{ts.Days}天{ts.Hours}小时{ts.Minutes}分{ts.Seconds}秒"},
            {"框架平台", SystemPlatformInfo.FrameworkDescription},
            {"框架版本", SystemPlatformInfo.FrameworkVersion},
            {"CPU核心数", SystemPlatformInfo.ProcessorCount},
            {"CPU架构", SystemPlatformInfo.ProcessArchitecture}
        };
    }

    public NetworkInfo[] NetworkInfos { get; } = NetworkInfo.GetRealNetworkInfos();

    public DiskInfo[] DiskInfos { get; } = DiskInfo.GetDisks();

    public Dictionary<string, object> SystemPlatformDict { get; }
}