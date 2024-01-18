using System;
using System.ServiceProcess;

namespace Assistant.Model;

public class WinSvc
{
    public string? ServiceName => ServiceController?.ServiceName ?? "N/A";
    public string? DisplayName => ServiceController?.DisplayName ?? "N/A";
    public ServiceStartMode? StartType => ServiceController?.StartType;
    public string StartTypeStr => ServiceStartModeToString(StartType);
    public ServiceControllerStatus? Status => ServiceController?.Status;
    public string StatusStr => ServiceControllerStatusToString(Status);


    public ServiceController? ServiceController { get; set; }


    private string ServiceControllerStatusToString(ServiceControllerStatus? status)
    {
        return status switch
        {
            ServiceControllerStatus.Stopped => "服务未运行",
            ServiceControllerStatus.StartPending => "服务正在启动",
            ServiceControllerStatus.StopPending => "服务正在停止",
            ServiceControllerStatus.Running => "服务正在运行",
            ServiceControllerStatus.ContinuePending => "服务即将继续",
            ServiceControllerStatus.PausePending => "服务即将暂停",
            ServiceControllerStatus.Paused => "服务已暂停",
            _ => "未知"
        };
    }

    private string ServiceStartModeToString(ServiceStartMode? mode)
    {
        return mode switch
        {
            ServiceStartMode.Boot => "Boot",
            ServiceStartMode.System => "System",
            ServiceStartMode.Automatic => "自动",
            ServiceStartMode.Manual => "手动",
            ServiceStartMode.Disabled => "禁用",
            _ => "未知"
        };
    }


    /// <summary>
    /// TODO 从WMI中获取更详细的信息
    /// </summary>
    public class WmiInfo
    {
        public string? BinPath { get; set; }
        public int? Pid { get; set; }
    }
}