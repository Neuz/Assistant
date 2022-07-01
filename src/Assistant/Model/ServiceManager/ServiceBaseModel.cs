using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Assistant.Model.ServiceManager;

public enum RunningStatus
{
    [Description("运行中")]
    Running,
    [Description("停止")]
    Stopped,
    [Description("未知")]
    UnKnown
}

public class ServiceBaseModel
{
    public string? DisplayName { get; set; }

    public string? Version { get; set; }

    /// <summary>
    /// Windows服务名
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Windows服务 exe path
    /// </summary>
    public string? BinPath { get; set; }

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
    /// 配置文件路径
    /// </summary>
    public string? ConfigFilePath { get; set; }

    /// <summary>
    /// 安装状态
    /// </summary>
    [JsonIgnore]
    public bool Installed { get; set; }

    /// <summary>
    /// 运行状态
    /// </summary>
    [JsonIgnore]
    public RunningStatus RunningStatus { get; set; }

    [JsonIgnore]
    public string InsConfFilePath => ServiceDirectory == null ? string.Empty : Path.Combine(ServiceDirectory, Global.InstallConfFileName);
}