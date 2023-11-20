using System.ServiceProcess;
using Assistant.Model.BaseServices;
using Syncfusion.Licensing;
using static System.String;

namespace Assistant.Model.OperationsTools;

public class WinSvc
{
    public string? ServiceName { get; set; } = Empty;
    public string? DisplayName { get; set; } = Empty;
    public string? Description { get; set; } = Empty;
    public int? Pid { get; set; } = null;
    /// <summary>
    /// 可执行文件路径
    /// </summary>
    public string? BinPath { get; set; } = Empty;

    /// <summary>
    /// 启动类型
    /// </summary>
    public StartMode? StartType { get; init; }

    /// <summary>
    /// 运行状态
    /// </summary>
    public RunningStatus? Status { get; init; }


    public enum StartMode
    {
        Boot      = 0,
        System    = 1,
        Automatic = 2,
        Manual    = 3,
        Disabled  = 4,
    }

    public enum RunningStatus
    {
        Stopped         = 1,
        StartPending    = 2,
        StopPending     = 3,
        Running         = 4,
        ContinuePending = 5,
        PausePending    = 6,
        Paused          = 7,
    }
}