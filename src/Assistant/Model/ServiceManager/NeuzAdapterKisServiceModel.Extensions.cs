using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assistant.Utils;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Serilog;

// ReSharper disable ReplaceWithSingleCallToAny

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public static class NeuzAdapterKisServiceModelExtensions
{
    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<NeuzAdapterKisServiceModel> Flush(this NeuzAdapterKisServiceModel model)
    {
        if (File.Exists(model.InsConfFilePath))
            model = await FileUtils.ReadFromConf<NeuzAdapterKisServiceModel>(model.InsConfFilePath) ?? new NeuzAdapterKisServiceModel();
        else
            model = new NeuzAdapterKisServiceModel();

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
    /// <param name="zipFilePath"></param>
    /// <returns></returns>
    public static async Task<bool> Install(this NeuzAdapterKisServiceModel model, string? zipFilePath)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(model.BinPath, nameof(model.BinPath));
            ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
            ArgumentNullException.ThrowIfNull(model.ConfigFilePath, nameof(model.ConfigFilePath));
            ArgumentNullException.ThrowIfNull(model.ServiceDirectory, nameof(model.ServiceDirectory));
            ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));

            if ((!Directory.Exists(model.ServiceDirectory) || !Directory.EnumerateFileSystemEntries(model.ServiceDirectory).Any()) && string.IsNullOrEmpty(zipFilePath))
                throw new FileNotFoundException($"{model.ServiceDirectory} 目录不存在或为空目录，请选择软件包\r\n{model.ServiceDirectory}");

            // 选择软件包
            if (!string.IsNullOrEmpty(zipFilePath))
            {
                // 备份 Adapter
                ArgumentNullException.ThrowIfNull(zipFilePath, nameof(zipFilePath));

                if (!Directory.Exists(model.PackHistoryDirectory))
                {
                    Log.Information($"create directory:{model.PackHistoryDirectory}");
                    Directory.CreateDirectory(model.PackHistoryDirectory);
                }

                var zipFileInfo = new FileInfo(zipFilePath);
                var zipFileExists = Directory.GetFiles(model.PackHistoryDirectory)
                                             .Select(f => new FileInfo(f).Name)
                                             .Where(s => s.Equals(zipFileInfo.Name, StringComparison.OrdinalIgnoreCase))
                                             .Any();
                if (!zipFileExists)
                {
                    var destZipFilePath = Path.Combine(model.PackHistoryDirectory, zipFileInfo.Name);
                    File.Copy(zipFilePath, destZipFilePath);
                }

                // 备份现有 Adapter
                if (!Directory.Exists(model.BackupDirectory))
                {
                    Log.Information($"create directory:{model.BackupDirectory}");
                    Directory.CreateDirectory(model.BackupDirectory);
                }

                if (Directory.Exists(model.ServiceDirectory) && Directory.EnumerateFileSystemEntries(model.ServiceDirectory).Any())
                {
                    var appBkFileName = $"Adapter.kisu.{DateTime.Now:yyyyMMddhhmmss}.zip";
                    var appBkFilePath = Path.Combine(model.BackupDirectory, appBkFileName);
                    await FileUtils.ZipFromDirectory(model.ServiceDirectory, appBkFilePath);
                }

                // 解压zip
                // 这里压缩包带有一层目录
                var destDir = Directory.GetParent(model.ServiceDirectory)?.FullName;
                await FileUtils.ZipExtractToDirectory(zipFilePath, destDir);

                // 获取API版本号
                var split = zipFileInfo.Name.Split(".zip")[0].Split("_");
                model.Version = split.Length > 1 ? split[1] : split[0];
            }


            // 备份配置文件
            if (File.Exists(model.ConfigFilePath))
            {
                var rs = await FileUtils.BackupFile(model.ConfigFilePath);
                if (!rs) return rs;
            }

            // 写入配置文件
            var text = model.ToConfigString();
            await FileUtils.WriteToFile(text, model.ConfigFilePath);

            // 写入ins.conf
            var insConfPath = Path.Combine(model.ServiceDirectory, Global.InstallConfFileName);
            await FileUtils.WriteToFile(model, insConfPath);

            if (!Directory.Exists(model.LogDirectory)) Directory.CreateDirectory(model.LogDirectory);

            // 创建Windows服务
            var binPath = $"{model.BinPath} --service";
            return await WinServiceUtils.CreateService(binPath, model.ServiceName, $"{model.ServiceName} 服务", $"{model.ServiceDescription}");
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return false;
        }
    }

    /// <summary>
    /// 卸载
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<bool> UnInstall(this NeuzAdapterKisServiceModel model)
    {
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));
        try
        {
            var rs = await WinServiceUtils.DeleteService(model.ServiceName);
            if (!rs) return rs;
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