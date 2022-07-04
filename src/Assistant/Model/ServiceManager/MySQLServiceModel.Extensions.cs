using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Assistant.Utils;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;
using Serilog;

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
        if (File.Exists(model.InsConfFilePath))
            model = await FileUtils.ReadFromConf<MySQLServiceModel>(model.InsConfFilePath) ?? new MySQLServiceModel();
        else
            model = new MySQLServiceModel();

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
    public static async Task<bool> Install(this MySQLServiceModel model)
    {
        ArgumentNullException.ThrowIfNull(model.BinPath, nameof(model.BinPath));
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.ConfigFilePath, nameof(model.ConfigFilePath));
        ArgumentNullException.ThrowIfNull(model.ServiceDirectory, nameof(model.ServiceDirectory));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));

        // 创建目录
        if (!Directory.Exists(model.TempDirectory)) Directory.CreateDirectory(model.TempDirectory);
        if (!Directory.Exists(model.LogDirectory)) Directory.CreateDirectory(model.LogDirectory);


        // mysql.exe初始化
        if (!File.Exists(model.MySQLExePath))
        {
            Log.Error($"文件不存在 {model.MySQLExePath}");
            return false;
        }

        // 备份配置文件
        if (File.Exists(model.ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(model.ConfigFilePath);
            if (!rs) return rs;
        }


        // 写入新配置文件
        var iniText = model.GetIniText(true);
        await File.WriteAllTextAsync(model.ConfigFilePath, iniText, new UTF8Encoding(false));
        Log.Information($"create file: {model.ConfigFilePath}");

        Log.Information("开始执行MySQL初始化");
        var cli = Cli.Wrap(model.BinPath)
                     .WithArguments(new[] {"--initialize", "--user=mysql", "--console"})
                     .WithValidation(CommandResultValidation.None);

        var cliResult = await cli.ExecuteBufferedAsync();
        Log.Information(cliResult.StandardOutput);
        Log.Warning(cliResult.StandardError);


        // 创建windows服务
        var binPath = @$"{model.BinPath} --defaults-file=""{model.ConfigFilePath}"" {model.ServiceName}";
        await WinServiceUtils.CreateService(binPath, model.ServiceName, model.ServiceDescription ?? string.Empty, model.ServiceDescription ?? string.Empty);
        Log.Information($"windows 服务创建成功: {model.ServiceName}");

        // 启动服务
        await WinServiceUtils.StartService(model.ServiceName);

        // 设置密码
        var sqlText = model.GetResetSql();
        var sqlPath = Path.Combine(model.ServiceDirectory, "reset.sql");
        await File.WriteAllTextAsync(sqlPath, sqlText, new UTF8Encoding(false));
        // $" -h localhost -P {MySQLService.Port} < {resetSqlPath}"
        cli = Cli.Wrap("cmd.exe")
                 .WithArguments(new[] {"/c", model.MySQLExePath, "-h", "localhost", "-P", $"{model.MySQLConfig.Port}", "<", sqlPath})
                 .WithValidation(CommandResultValidation.ZeroExitCode);
        cliResult = await cli.ExecuteBufferedAsync();
        Log.Information(cliResult.StandardOutput);
        Log.Warning(cliResult.StandardError);

        if (File.Exists(sqlPath)) File.Delete(sqlPath);

        // 写入新配置文件
        iniText = model.GetIniText();
        await File.WriteAllTextAsync(model.ConfigFilePath, iniText, new UTF8Encoding(false));
        Log.Information($"create file: {model.ConfigFilePath}");

        await WinServiceUtils.StopService(model.ServiceName);

        // 写入ins.conf
        var insConfPath = Path.Combine(model.ServiceDirectory, Global.InstallConfFileName);
        await FileUtils.WriteToFile(model, insConfPath);

        return true;
    }

    /// <summary>
    /// 卸载
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<bool> UnInstall(this MySQLServiceModel model)
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