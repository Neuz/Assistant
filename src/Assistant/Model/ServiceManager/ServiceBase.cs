using Assistant.Utils;
using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Assistant.Model.ServiceManager;

public class ServiceBase : IService
{
    public string? DisplayName { get; set; }

    public string? Version { get; set; }

    public string? ServiceName { get; set; }

    public string? BinPath { get; set; }

    public string? ServiceDescription { get; set; }

    public string? ServiceDirectory { get; set; }

    public string? LogDirectory { get; set; }

    public string? ConfigFilePath { get; set; }

    [JsonIgnore]
    public bool Installed { get; set; }

    [JsonIgnore]
    public RunningStatus RunningStatus { get; set; }

    /// <summary>
    /// InsConf 文件路径
    /// </summary>
    [JsonIgnore]
    public string InsConfFilePath => ServiceDirectory == null ? string.Empty : Path.Combine(ServiceDirectory, Global.InstallConfFileName);


    public Task Start(Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(ServiceName, nameof(ServiceName));
        return WinServiceUtils.StartService(ServiceName);
    }

    public Task Stop(Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(ServiceName, nameof(ServiceName));
        return WinServiceUtils.StopService(ServiceName);
    }

    public virtual Task Install(Action<string>? infoAction = null)
    {
        throw new NotImplementedException();
    }

    public virtual Task UnInstall(Action<string>? infoAction = null)
    {
        throw new NotImplementedException();
    }
}