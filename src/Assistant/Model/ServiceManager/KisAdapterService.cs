using Assistant.Utils;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable ReplaceWithSingleCallToAny


namespace Assistant.Model.ServiceManager;

public partial class KisAdapterService : ServiceBase
{
    public override async Task Install(Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(BinPath, nameof(BinPath));
        ArgumentNullException.ThrowIfNull(ServiceName, nameof(ServiceName));
        ArgumentNullException.ThrowIfNull(ConfigFilePath, nameof(ConfigFilePath));
        ArgumentNullException.ThrowIfNull(ServiceDirectory, nameof(ServiceDirectory));
        ArgumentNullException.ThrowIfNull(LogDirectory, nameof(LogDirectory));

        if ((!Directory.Exists(ServiceDirectory) || !Directory.EnumerateFileSystemEntries(ServiceDirectory).Any()) && string.IsNullOrEmpty(ZipFilePath))
            throw new FileNotFoundException($"{ServiceDirectory} 目录不存在或为空目录，请选择软件包\r\n{ServiceDirectory}");

        // 备份现有 Adapter
        if (Directory.Exists(ServiceDirectory))
        {
            infoAction?.Invoke($"创建目录 [{BackupDirectory}]");
            Directory.CreateDirectory(BackupDirectory);
            var bkFilePath = Path.Combine(BackupDirectory, $"Adapter.k3wise.{DateTime.Now:yyyyMMddhhmmss}.zip");
            await FileUtils.ZipFromDirectory(ServiceDirectory, bkFilePath);
            infoAction?.Invoke($"备份当前 Adapter 完成 [{bkFilePath}]");
        }

        infoAction?.Invoke($"创建目录 [{ServiceDirectory}]");
        Directory.CreateDirectory(ServiceDirectory);

        // 更新文件
        if (!string.IsNullOrEmpty(ZipFilePath) && File.Exists(ZipFilePath))
        {
            // 解压zip
            infoAction?.Invoke($"正在解压 [{ZipFilePath}]");

            using var archive    = ZipFile.OpenRead(ZipFilePath);
            var separatorChar = archive.Entries.Where(e => e.FullName.Contains(Path.DirectorySeparatorChar)).Any()
                                    ? Path.DirectorySeparatorChar
                                    : Path.AltDirectorySeparatorChar;
            var startFlag = $"kisu{separatorChar}"; // 识别标识

            var       hasAdapter = archive.Entries.Where(e => e.FullName.StartsWith(startFlag,StringComparison.OrdinalIgnoreCase)).Any();
            if (!hasAdapter) throw new ApplicationException("当前文件不是 NeuzWiseAdapter 安装文件");
            foreach (var entry in archive.Entries)
            {
                if (!entry.FullName.StartsWith(startFlag, StringComparison.OrdinalIgnoreCase)) continue;

                var split = entry.FullName.Split(separatorChar);
                if (string.IsNullOrEmpty(entry.Name))
                {
                    var dir = Path.Combine(ServiceDirectory, Path.Combine(split[1..]));
                    Directory.CreateDirectory(dir);
                }
                else
                {
                    var filePath = Path.Combine(ServiceDirectory, Path.Combine(split[1..]));
                    entry.ExtractToFile(Path.Combine(ServiceDirectory, filePath), true);
                    infoAction?.Invoke($"解压文件 [{filePath}]");
                }
            }

            infoAction?.Invoke($"解压完成 [{ZipFilePath}]");
        }

        // 备份配置文件
        if (File.Exists(ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(ConfigFilePath);
            infoAction?.Invoke($"备份配置文件 [{rs}]");
        }

        // 更新配置文件
        var text = ToConfigString();
        await FileUtils.WriteToFile(text, ConfigFilePath);
        infoAction?.Invoke($"更新配置文件 [{ConfigFilePath}]");

        // 创建Windows服务
        var binPath = $"{BinPath} --service";
        await WinServiceUtils.CreateService(binPath, ServiceName, $"{ServiceName} 服务", $"{ServiceDescription}");
        infoAction?.Invoke($"创建windows服务: {ServiceName}");

        // 写入ins.conf
        await FileUtils.WriteToFile(this, InsConfFilePath);
        infoAction?.Invoke($"写入ins.conf: [{InsConfFilePath}]");
    }

    public override async Task UnInstall(Action<string>? infoAction = null)
    {
        if (File.Exists(InsConfFilePath))
        {
            File.Delete(InsConfFilePath);
            infoAction?.Invoke($"删除ins.conf [{InsConfFilePath}]");
        }

        infoAction?.Invoke($"删除Windows服务 [{ServiceName}]");
        await WinServiceUtils.DeleteService(ServiceName);
    }
}

public partial class KisAdapterService
{
    public KisAdapterService()
    {
        var baseDir = Path.Combine(Global.CurrentDir, "Adapters", "kisu");
        DisplayName        = "Neuz 适配器 for Kis Ultimate";
        Version            = "";
        ServiceName        = "Neuz.Adapter.KisUltimate";
        BinPath            = Path.Combine(baseDir, "NeuzKisAdapter.exe");
        ServiceDescription = "Neuz 适配器 for Kis Ultimate";
        ServiceDirectory   = baseDir;
        LogDirectory       = Path.Combine(baseDir, "Logs");
        ConfigFilePath     = Path.Combine(baseDir, "app.properties");
        Installed          = false;
        RunningStatus      = RunningStatus.UnKnown;
    }

    public int Port { get; set; } = 10005;
    public string BackupDirectory { get; set; } = Path.Combine(Global.BackupsDir, "Adapters_kisu");

    public string? ZipFilePath { get; set; }

    public KingdeeAccountModel KingdeeAccount { get; set; } = new();


    public string ToConfigString()
    {
        return string.Format(_configTemplate, Port, KingdeeAccount.Number, KingdeeAccount.User, KingdeeAccount.Password);
    }

    private string _configTemplate = @"app.port={0}
erp.acctId={1}
erp.acctUserName={2}
erp.acctPassword={3}";

    public class KingdeeAccountModel
    {
        public string Number { get; set; } = "";
        public string User { get; set; } = "administrator";
        public string Password { get; set; } = "";
    }
}