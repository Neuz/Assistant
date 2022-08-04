using Assistant.Utils;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

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
        var rs = File.Exists(model.InsConfFilePath)
                     ? await FileUtils.ReadFromConf<NeuzAdapterKisServiceModel>(model.InsConfFilePath) ?? throw new ApplicationException("刷新时序列化失败")
                     : new NeuzAdapterKisServiceModel();

        var installed = await WinServiceUtils.IsInstalled(model.ServiceName!);
        var status    = await WinServiceUtils.GetRunningStatus(model.ServiceName!);
        rs.Installed     = installed;
        rs.RunningStatus = status;
        return rs;
    }

    /// <summary>
    /// 安装
    /// </summary>
    /// <param name="model"></param>
    /// <param name="zipFilePath"></param>
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public static async Task Install(this NeuzAdapterKisServiceModel model, string? zipFilePath, Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(model.BinPath, nameof(model.BinPath));
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.ConfigFilePath, nameof(model.ConfigFilePath));
        ArgumentNullException.ThrowIfNull(model.ServiceDirectory, nameof(model.ServiceDirectory));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));
        ArgumentNullException.ThrowIfNull(zipFilePath, nameof(zipFilePath));

        if ((!Directory.Exists(model.ServiceDirectory) || !Directory.EnumerateFileSystemEntries(model.ServiceDirectory).Any()) && string.IsNullOrEmpty(zipFilePath))
            throw new FileNotFoundException($"{model.ServiceDirectory} 目录不存在或为空目录，请选择软件包\r\n{model.ServiceDirectory}");

        // 备份现有 Adapter
        if (Directory.Exists(model.ServiceDirectory))
        {
            infoAction?.Invoke($"创建目录 [{model.BackupDirectory}]");
            Directory.CreateDirectory(model.BackupDirectory);
            var bkFilePath = Path.Combine(model.BackupDirectory, $"Adapter.k3wise.{DateTime.Now:yyyyMMddhhmmss}.zip");
            await FileUtils.ZipFromDirectory(model.ServiceDirectory, bkFilePath);
            infoAction?.Invoke($"备份当前 Adapter 完成 [{bkFilePath}]");
        }

        infoAction?.Invoke($"创建目录 [{model.ServiceDirectory}]");
        Directory.CreateDirectory(model.ServiceDirectory);

        // 更新文件
        if (!string.IsNullOrEmpty(zipFilePath) && File.Exists(zipFilePath))
        {
            // 解压zip
            infoAction?.Invoke($"正在解压 [{zipFilePath}]");

            var startFlag = "KisU/"; // 识别标识

            using var archive    = ZipFile.OpenRead(zipFilePath);
            var       hasAdapter = archive.Entries.Where(e => e.FullName.StartsWith(startFlag)).Any();
            if (!hasAdapter) throw new ApplicationException("当前文件不是 NeuzWiseAdapter 安装文件");
            foreach (var entry in archive.Entries)
            {
                if (!entry.FullName.StartsWith(startFlag, StringComparison.OrdinalIgnoreCase)) continue;

                var split = entry.FullName.Split("/");
                if (string.IsNullOrEmpty(entry.Name))
                {
                    var dir = Path.Combine(model.ServiceDirectory, Path.Combine(split[1..]));
                    Directory.CreateDirectory(dir);
                }
                else
                {
                    var filePath = Path.Combine(model.ServiceDirectory, Path.Combine(split[1..]));
                    entry.ExtractToFile(Path.Combine(model.ServiceDirectory, filePath), true);
                    infoAction?.Invoke($"解压文件 [{filePath}]");
                }
            }

            infoAction?.Invoke($"解压完成 [{zipFilePath}]");
        }

        // 备份配置文件
        if (File.Exists(model.ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(model.ConfigFilePath);
            infoAction?.Invoke($"备份配置文件 [{rs}]");
        }

        // 更新配置文件
        var text = model.ToConfigString();
        await FileUtils.WriteToFile(text, model.ConfigFilePath);
        infoAction?.Invoke($"更新配置文件 [{model.ConfigFilePath}]");

        // 创建Windows服务
        var binPath = $"{model.BinPath} --service";
        await WinServiceUtils.CreateService(binPath, model.ServiceName, $"{model.ServiceName} 服务", $"{model.ServiceDescription}");
        infoAction?.Invoke($"创建windows服务: {model.ServiceName}");

        // 写入ins.conf
        await FileUtils.WriteToFile(model, model.InsConfFilePath);
        infoAction?.Invoke($"写入ins.conf: [{model.InsConfFilePath}]");
    }

    /// <summary>
    /// 卸载
    /// </summary>
    /// <param name="model"></param>
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public static async Task UnInstall(this NeuzAdapterKisServiceModel model, Action<string>? infoAction = null)
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