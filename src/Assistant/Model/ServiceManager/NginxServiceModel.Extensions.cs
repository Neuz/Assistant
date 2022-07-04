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
    /// <returns></returns>
    public static async Task<bool> Install(this NginxServiceModel model)
    {
        ArgumentNullException.ThrowIfNull(model.BinPath, nameof(model.BinPath));
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.ConfigFilePath, nameof(model.ConfigFilePath));
        ArgumentNullException.ThrowIfNull(model.ServiceDirectory, nameof(model.ServiceDirectory));
        ArgumentNullException.ThrowIfNull(model.TempDirectory, nameof(model.TempDirectory));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));

        // 备份配置文件
        if (File.Exists(model.ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(model.ConfigFilePath);
            if (!rs) return rs;
        }

        // 写入配置文件
        var a = model.NginxConfig.ToString();
        await File.WriteAllTextAsync(model.ConfigFilePath, a, new UTF8Encoding(false));

        // 写入ins.conf
        var insConfPath = Path.Combine(model.ServiceDirectory, Global.InstallConfFileName);
        await FileUtils.WriteToConf(model, insConfPath);

        if (!Directory.Exists(model.TempDirectory)) Directory.CreateDirectory(model.TempDirectory);
        if (!Directory.Exists(model.LogDirectory)) Directory.CreateDirectory(model.LogDirectory);

        // 创建Windows服务
        return await WinServiceUtils.CreateService(model.BinPath, model.ServiceName, model.ServiceDescription ?? string.Empty, model.ServiceDescription ?? string.Empty);
    }

    /// <summary>
    /// 卸载
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<bool> UnInstall(this NginxServiceModel model)
    {
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));
        try
        {
            var rs = await WinServiceUtils.DeleteService(model.ServiceName);
            if (!rs) return rs;
            if (Directory.Exists(model.TempDirectory)) Directory.Delete(model.TempDirectory, true);
            if (Directory.Exists(model.LogDirectory)) Directory.Delete(model.LogDirectory, true);
            if (File.Exists(model.InsConfFilePath)) File.Delete(model.InsConfFilePath);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return false;
        }
    }
}