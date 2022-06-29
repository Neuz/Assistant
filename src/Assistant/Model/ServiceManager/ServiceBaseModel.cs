using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Assistant.Model.ServiceManager;

public enum RunningStatus
{
    [Description("运行中")]
    Running,
    [Description("停止")]
    Stopping,
    [Description("未知")]
    UnKnown
}

public class ServiceBaseModel
{
    public string? DisplayName { get; set; }

    /// <summary>
    /// Windows服务名
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Windows服务 exe path
    /// </summary>
    public string? ServicePath { get; set; }

    /// <summary>
    /// Windows服务描述
    /// </summary>
    public string? ServiceDescription { get; set; }

    /// <summary>
    /// 服务安装目录
    /// </summary>
    public string? ServiceDirectory { get; set; }

    /// <summary>
    /// 服务日志目录
    /// </summary>
    public string? LogDirectory { get; set; }

    /// <summary>
    /// 安装状态
    /// </summary>
    public bool Installed { get; set; }

    /// <summary>
    /// 运行状态
    /// </summary>
    public RunningStatus RunningStatus { get; set; }
}