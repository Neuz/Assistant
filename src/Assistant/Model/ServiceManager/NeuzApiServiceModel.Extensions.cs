using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assistant.Utils;
using CliWrap;
using CliWrap.Buffered;
using MySqlConnector;
using Serilog;
using Syncfusion.Windows.Tools;

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public static class NeuzApiServiceModelExtensions
{
    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<NeuzApiServiceModel> Flush(this NeuzApiServiceModel model)
    {
        if (File.Exists(model.InsConfFilePath))
            model = await FileUtils.ReadFromConf<NeuzApiServiceModel>(model.InsConfFilePath) ?? new NeuzApiServiceModel();
        else
            model = new NeuzApiServiceModel();

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
    /// <param name="processHandler"></param>
    /// <returns></returns>
    public static async Task<bool> Install(this NeuzApiServiceModel model, string zipFilePath = "", Action<int>? processHandler = null)
    {
        ArgumentNullException.ThrowIfNull(model.BinPath, nameof(model.BinPath));
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        ArgumentNullException.ThrowIfNull(model.ConfigFilePath, nameof(model.ConfigFilePath));
        ArgumentNullException.ThrowIfNull(model.ServiceDirectory, nameof(model.ServiceDirectory));
        ArgumentNullException.ThrowIfNull(model.LogDirectory, nameof(model.LogDirectory));

        // 创建目录
        if (!Directory.Exists(model.ServiceDirectory)) Directory.CreateDirectory(model.ServiceDirectory);
        if (!Directory.Exists(model.LogDirectory)) Directory.CreateDirectory(model.LogDirectory);

        // 处理更新包
        if (!string.IsNullOrEmpty(zipFilePath))
        {
            // todo  备份/解压/覆盖
        }

        try
        {
            var builder = model.GetMySqlConnectionStringBuilder();
            builder.ConnectionTimeout = 10;
            if (!MySQLUtils.TestConnect(builder.ToString())) throw new ApplicationException("MySQL连接失败");

            // 执行创建数据库脚本
            var createDbSQLPath = Path.Combine(Global.SQLDir, "create_db.sql");
            var sqlStr          = model.GetCreateDbSQL();
            await FileUtils.WriteToFile(sqlStr, createDbSQLPath);

            Log.Information($"Import file: {createDbSQLPath}");
            builder.AllowUserVariables = true;
            MySQLUtils.ImportSQL(builder.ToString(), createDbSQLPath);

            // 处理初始化SQL
            // 这里支持格式  [排序]_Script_[数据库名].sql  
            var filePaths = Directory.GetFiles(Global.SQLDir, "*.sql")
                                     .Where(s => !s.Equals(createDbSQLPath))
                                     .Select(f =>
                                      {
                                          var fi    = new FileInfo(f);
                                          var ff    = fi.Name.Substring(0, fi.Name.LastIndexOf(fi.Extension, StringComparison.OrdinalIgnoreCase));
                                          var split = ff.Split("_Script_");
                                          if (split.Length < 1) return null;
                                          return new
                                          {
                                              order  = split[0],
                                              dbName = split[1],
                                              orgin  = f
                                          };
                                      })
                                     .Where(s => s != null)
                                     .ToArray();
            // neuz 库
            var neuzFilePaths = filePaths.Where(s => s!.dbName.Equals("neuz", StringComparison.OrdinalIgnoreCase))
                                         .OrderBy(s => s!.order)
                                         .Select(s => s!.orgin)
                                         .ToArray();
            builder.Database = string.IsNullOrEmpty(model.DbPrefix) ? "neuz" : $"{model.DbPrefix}_neuz";

            await MySQLUtils.ImportSQL(builder.ToString(), neuzFilePaths, null,
                filePath => { Log.Information($"Import file: {filePath}"); },
                (o, args) =>
                {
                    Log.Information($"{args.PercentageCompleted}%");
                    processHandler?.Invoke(args.PercentageCompleted);
                });

            // neuz_saas 库
            var neuzSaaSFilePaths = filePaths.Where(s => s!.dbName.Equals("neuz_saas", StringComparison.OrdinalIgnoreCase))
                                             .OrderBy(s => s!.order)
                                             .Select(s => s!.orgin)
                                             .ToArray();
            builder.Database = string.IsNullOrEmpty(model.DbPrefix) ? "neuz_saas" : $"{model.DbPrefix}_neuz_saas";

            await MySQLUtils.ImportSQL(builder.ToString(), neuzSaaSFilePaths, null,
                filePath => { Log.Information($"Import file: {filePath}"); },
                (o, args) =>
                {
                    Log.Information($"{args.PercentageCompleted}%");
                    processHandler?.Invoke(args.PercentageCompleted);
                });

            // neuz_log 库
            var neuzLogFilePaths = filePaths.Where(s => s!.dbName.Equals("neuz_log", StringComparison.OrdinalIgnoreCase))
                                            .OrderBy(s => s!.order)
                                            .Select(s => s!.orgin)
                                            .ToArray();
            builder.Database = string.IsNullOrEmpty(model.DbPrefix) ? "neuz_log" : $"{model.DbPrefix}_neuz_log";

            await MySQLUtils.ImportSQL(builder.ToString(), neuzLogFilePaths, null,
                filePath => { Log.Information($"Import file: {filePath}"); },
                (o, args) =>
                {
                    Log.Information($"{args.PercentageCompleted}%");
                    processHandler?.Invoke(args.PercentageCompleted);
                });
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }

        // 备份配置文件
        if (File.Exists(model.ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(model.ConfigFilePath);
            if (!rs) return rs;
        }


        // 写入新配置文件
        var iniText = model.GetDbSettingsSerializer();
        await File.WriteAllTextAsync(model.ConfigFilePath, iniText, new UTF8Encoding(false));
        Log.Information($"create file: {model.ConfigFilePath}");


        // 创建windows服务
        // todo 这里没放https
        // var binPath = @$"{model.BinPath} --urls ""http://*:8001;https://*:8002""";
        var binPath = @$"{model.BinPath} --urls ""http://*:{model.Port}""";
        await WinServiceUtils.CreateService(binPath, model.ServiceName, model.ServiceDescription ?? string.Empty, model.ServiceDescription ?? string.Empty);
        Log.Information($"windows 服务创建成功: {model.ServiceName}");


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
    public static async Task<bool> UnInstall(this NeuzApiServiceModel model)
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


    public static MySqlConnectionStringBuilder GetMySqlConnectionStringBuilder(this NeuzApiServiceModel model)
    {
        // https://www.connectionstrings.com/mysql/
        return new MySqlConnectionStringBuilder
        {
            Server       = model.MySQLHost,
            Port         = Convert.ToUInt32(model.MySQLPort),
            UserID       = model.MySQLUser,
            Password     = model.MySQLPassword,
            CharacterSet = "utf8mb4"
        };
    }
}