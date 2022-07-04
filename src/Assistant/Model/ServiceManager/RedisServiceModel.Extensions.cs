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
    /// <returns></returns>
    public static async Task<bool> Install(this RedisServiceModel model)
    {
        ArgumentNullException.ThrowIfNull(model.BinPath, nameof(model.BinPath));
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.ConfigFilePath, nameof(model.ConfigFilePath));
        ArgumentNullException.ThrowIfNull(model.ServiceDirectory, nameof(model.ServiceDirectory));

        // 备份配置文件
        if (File.Exists(model.ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(model.ConfigFilePath);
            if (!rs) return rs;
        }

        // 写入新配置文件
        var text = model.GetConfigText();
        await File.WriteAllTextAsync(model.ConfigFilePath, text, new UTF8Encoding(false));
        Log.Information($"create file: {model.ConfigFilePath}");

        // 写入ins.conf
        var insConfPath = Path.Combine(model.ServiceDirectory, Global.InstallConfFileName);
        await FileUtils.WriteToFile(model, insConfPath);

        // 创建Windows服务
        var binPath = @$"""{model.BinPath}"" --service-run ""{model.ConfigFilePath}""";
        return await WinServiceUtils.CreateService(binPath, model.ServiceName, model.ServiceDescription ?? string.Empty, model.ServiceDescription ?? string.Empty);
    }

    /// <summary>
    /// 卸载
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<bool> UnInstall(this RedisServiceModel model)
    {
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));
        try
        {
            var rs = await WinServiceUtils.DeleteService(model.ServiceName);
            if (!rs) return rs;
            if (File.Exists(model.InsConfFilePath)) File.Delete(model.InsConfFilePath);
            var logFilePath = Path.Combine(model.LogDirectory, "redis_server.log");
            if (File.Exists(logFilePath)) File.Delete(logFilePath);
            if (File.Exists(model.ConfigFilePath)) File.Delete(model.ConfigFilePath);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return false;
        }
    }
}