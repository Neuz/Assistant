using Assistant.Utils;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public static class NginxServiceModelExtensions
{
    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<NginxServiceModel> Flush(this NginxServiceModel model)
    {
        if (File.Exists(model.InsConfFilePath))
            model = await FileUtils.ReadFromConf<NginxServiceModel>(model.InsConfFilePath) ?? new NginxServiceModel();
        else
            model = new NginxServiceModel();

        var installed = await WinServiceUtils.IsInstalled(model.ServiceName!);
        var status    = await WinServiceUtils.GetRunningStatus(model.ServiceName!);
        var rs        = FileUtils.DeepClone(model);
        if (rs != null)
        {
            rs.Installed     = installed;
            rs.RunningStatus = status;
            return rs;
        }

        Log.Error("刷新时序列化失败");
        return model;
    }

    /// <summary>
    /// 安装
    /// </summary>
    /// <param name="model"></param>
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public static async Task Install(this NginxServiceModel model, Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(model.BinPath, nameof(model.BinPath));
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.ConfigFilePath, nameof(model.ConfigFilePath));
        ArgumentNullException.ThrowIfNull(model.ServiceDirectory, nameof(model.ServiceDirectory));
        ArgumentNullException.ThrowIfNull(model.TempDirectory, nameof(model.TempDirectory));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));

        if (!Directory.Exists(model.TempDirectory))
        {
            Directory.CreateDirectory(model.TempDirectory);
            infoAction?.Invoke($"创建目录 [{model.TempDirectory}]");
        }

        if (!Directory.Exists(model.LogDirectory))
        {
            Directory.CreateDirectory(model.LogDirectory);
            infoAction?.Invoke($"创建目录 [{model.LogDirectory}]");
        }

        // 备份配置文件
        if (File.Exists(model.ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(model.ConfigFilePath);
            infoAction?.Invoke($"备份配置文件 [{rs}]");
        }

        // 更新配置文件
        var contents = model.NginxConfig.ToString();
        await FileUtils.WriteToFile(contents, model.ConfigFilePath);
        infoAction?.Invoke($"更新配置文件 [{model.ConfigFilePath}]");


        // 写入ins.conf
        var insConfPath = Path.Combine(model.ServiceDirectory, Global.InstallConfFileName);
        await FileUtils.WriteToFile(model, insConfPath);
        infoAction?.Invoke($"写入ins.conf: [{insConfPath}]");


        // 创建Windows服务
        await WinServiceUtils.CreateService(model.BinPath, model.ServiceName, model.ServiceDescription ?? string.Empty, model.ServiceDescription ?? string.Empty);
        infoAction?.Invoke($"windows 服务创建成功: [{model.ServiceName}]");
    }

    /// <summary>
    /// 卸载
    /// </summary>
    /// <param name="model"></param>
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public static async Task UnInstall(this NginxServiceModel model, Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));

        infoAction?.Invoke($"删除Windows服务 [{model.ServiceName}]");
        await WinServiceUtils.DeleteService(model.ServiceName);

        if (Directory.Exists(model.TempDirectory))
        {
            Directory.Delete(model.TempDirectory, true);
            infoAction?.Invoke($"清理临时目录 [{model.TempDirectory}]");
        }

        if (File.Exists(model.InsConfFilePath))
        {
            File.Delete(model.InsConfFilePath);
            infoAction?.Invoke($"删除ins.conf [{model.InsConfFilePath}]");
        }
    }
}