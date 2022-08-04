using Assistant.Utils;
using CliWrap;
using CliWrap.Buffered;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace Assistant.Model.ServiceManager;

public static class MySQLServiceModelExtensions
{
    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<MySQLServiceModel> Flush(this MySQLServiceModel model)
    {
        var rs = File.Exists(model.InsConfFilePath) 
                    ? await FileUtils.ReadFromConf<MySQLServiceModel>(model.InsConfFilePath) ?? throw new ApplicationException("刷新时序列化失败") 
                    : new MySQLServiceModel();

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
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public static async Task Install(this MySQLServiceModel model, Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(model.BinPath, nameof(model.BinPath));
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.ConfigFilePath, nameof(model.ConfigFilePath));
        ArgumentNullException.ThrowIfNull(model.ServiceDirectory, nameof(model.ServiceDirectory));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));

        if (!File.Exists(model.MySQLExePath)) throw new ApplicationException($"文件不存在 {model.MySQLExePath}");
        if (!File.Exists(model.BinPath)) throw new ApplicationException($"文件不存在 {model.BinPath}");

        // 创建目录
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
        var iniText = model.GetIniText(true);
        await File.WriteAllTextAsync(model.ConfigFilePath, iniText, new UTF8Encoding(false));

        // mysql 初始化
        infoAction?.Invoke($"开始执行MySQL初始化 [{model.BinPath}]");
        var cli = Cli.Wrap(model.BinPath)
                     .WithArguments(new[] {"--initialize", "--user=mysql", "--console"})
                     .WithValidation(CommandResultValidation.None);
        infoAction?.Invoke(cli.ToString());
        var cliResult = await cli.ExecuteBufferedAsync();
        infoAction?.Invoke(cliResult.StandardOutput);
        infoAction?.Invoke(cliResult.StandardError);


        // 创建windows服务
        var binPath = @$"{model.BinPath} --defaults-file=""{model.ConfigFilePath}"" {model.ServiceName}";
        await WinServiceUtils.CreateService(binPath, model.ServiceName, model.ServiceDescription ?? string.Empty, model.ServiceDescription ?? string.Empty);
        infoAction?.Invoke($"windows 服务创建成功: [{model.ServiceName}]");

        // 启动服务
        await WinServiceUtils.StartService(model.ServiceName);
        infoAction?.Invoke($"windows 服务启动成功: [{model.ServiceName}]");


        // 设置密码
        infoAction?.Invoke("设置密码");
        var sqlText = model.GetResetSql();
        var sqlPath = Path.Combine(model.ServiceDirectory, "reset.sql");
        await File.WriteAllTextAsync(sqlPath, sqlText, new UTF8Encoding(false));
        cli = Cli.Wrap("cmd.exe")
                 .WithArguments(new[] {"/c", model.MySQLExePath, "-h", "localhost", "-P", $"{model.MySQLConfig.Port}", "<", sqlPath})
                 .WithValidation(CommandResultValidation.ZeroExitCode);
        cliResult = await cli.ExecuteBufferedAsync();
        infoAction?.Invoke(cliResult.StandardOutput);
        infoAction?.Invoke(cliResult.StandardError);

        if (File.Exists(sqlPath)) File.Delete(sqlPath);

        // 写入新配置文件
        iniText = model.GetIniText();
        await File.WriteAllTextAsync(model.ConfigFilePath, iniText, new UTF8Encoding(false));

        await WinServiceUtils.StopService(model.ServiceName);
        infoAction?.Invoke($"windows 服务停止成功: [{model.ServiceName}]");


        // 写入ins.conf
        var insConfPath = Path.Combine(model.ServiceDirectory, Global.InstallConfFileName);
        await FileUtils.WriteToFile(model, insConfPath);
        infoAction?.Invoke($"写入ins.conf: [{insConfPath}]");
    }

    /// <summary>
    /// 卸载
    /// </summary>
    /// <param name="model"></param>
    /// <param name="infoAction"></param>
    /// <returns></returns>
    public static async Task UnInstall(this MySQLServiceModel model, Action<string>? infoAction = null)
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