using Assistant.Utils;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public static class RedisServiceModelExtensions
{
    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<RedisServiceModel> Flush(this RedisServiceModel model)
    {
        if (File.Exists(model.InsConfFilePath))
            model = await FileUtils.ReadFromConf<RedisServiceModel>(model.InsConfFilePath) ?? new RedisServiceModel();
        else
            model = new RedisServiceModel();

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
    public static async Task Install(this RedisServiceModel model, Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(model.BinPath, nameof(model.BinPath));
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.ConfigFilePath, nameof(model.ConfigFilePath));
        ArgumentNullException.ThrowIfNull(model.ServiceDirectory, nameof(model.ServiceDirectory));

        // 备份配置文件
        if (File.Exists(model.ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(model.ConfigFilePath);
            infoAction?.Invoke($"备份配置文件 [{rs}]");
        }

        // 更新配置文件
        var text = model.GetConfigText();
        await File.WriteAllTextAsync(model.ConfigFilePath, text, new UTF8Encoding(false));
        infoAction?.Invoke($"更新配置文件 [{model.ConfigFilePath}]");

        // 写入ins.conf
        var insConfPath = Path.Combine(model.ServiceDirectory, Global.InstallConfFileName);
        await FileUtils.WriteToFile(model, insConfPath);
        infoAction?.Invoke($"写入ins.conf: [{insConfPath}]");

        // 创建Windows服务
        var binPath = @$"""{model.BinPath}"" --service-run ""{model.ConfigFilePath}""";
        await WinServiceUtils.CreateService(binPath, model.ServiceName, model.ServiceDescription ?? string.Empty, model.ServiceDescription ?? string.Empty);
        infoAction?.Invoke($"windows 服务创建成功: [{model.ServiceName}]");
    }

    /// <summary>
    /// 卸载
    /// </summary>
    /// <param name="model"></param>
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public static async Task UnInstall(this RedisServiceModel model, Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));

        infoAction?.Invoke($"删除Windows服务 [{model.ServiceName}]");
        await WinServiceUtils.DeleteService(model.ServiceName);
        if (File.Exists(model.InsConfFilePath))
        {
            File.Delete(model.InsConfFilePath);
            infoAction?.Invoke($"删除ins.conf [{model.InsConfFilePath}]");
        }
    }
}