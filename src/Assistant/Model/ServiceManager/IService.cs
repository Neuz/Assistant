using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Assistant.Model.ServiceManager;

public interface IService
{
    /// <summary>
    /// 显示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
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
    public bool Installed { get; set; }

    /// <summary>
    /// 运行状态
    /// </summary>
    public RunningStatus RunningStatus { get; set; }

    /// <summary>
    /// 启动服务
    /// </summary>
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public Task Start(Action<string>? infoAction = null);

    /// <summary>
    /// 停止服务
    /// </summary>
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public Task Stop(Action<string>? infoAction = null);

    /// <summary>
    /// 安装
    /// </summary>
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public Task Install(Action<string>? infoAction = null);

    /// <summary>
    /// 卸载
    /// </summary>
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public Task UnInstall(Action<string>? infoAction = null);
}

/// <summary>
/// 运行状态
/// </summary>
public enum RunningStatus
{
    [Description("运行中")]
    Running,

    [Description("停止")]
    Stopped,

    [Description("未知")]
    UnKnown
}