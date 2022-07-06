using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assistant.Utils;
using MySqlConnector;
using Serilog;

// ReSharper disable ReplaceWithSingleCallToAny

namespace Assistant.Model.ServiceManager;

public static class NeuzAppServiceModelExtensions
{
    public static async Task<NeuzAppServiceModel> Flush(this NeuzAppServiceModel app)
    {
        ArgumentNullException.ThrowIfNull(app.Api.ServiceName, nameof(app.Api.ServiceName));

        if (File.Exists(app.Api.InsConfFilePath))
            app.Api = await FileUtils.ReadFromConf<NeuzAppServiceModel.NeuzApiServiceModel>(app.Api.InsConfFilePath) ?? new NeuzAppServiceModel.NeuzApiServiceModel();
        else
            app.Api = new NeuzAppServiceModel.NeuzApiServiceModel();

        var installed = await WinServiceUtils.IsInstalled(app.Api.ServiceName!);
        var status    = await WinServiceUtils.GetRunningStatus(app.Api.ServiceName!);
        var rs        = FileUtils.DeepClone(app);
        if (rs == null)
        {
            Log.Error("刷新时序列化失败");
            return app;
        }

        rs.Api.Installed     = installed;
        rs.Api.RunningStatus = status;
        return rs;
    }

    public static async Task<bool> Install(this NeuzAppServiceModel model, string? zipFilePath)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(model.Api.ServiceDirectory, nameof(model.Api.ServiceDirectory));
            ArgumentNullException.ThrowIfNull(model.Api.LogDirectory, nameof(model.Api.LogDirectory));
            ArgumentNullException.ThrowIfNull(model.Api.ServiceName, nameof(model.Api.ServiceName));
            ArgumentNullException.ThrowIfNull(model.Directory, nameof(model.Directory));

            if ((!Directory.Exists(model.Directory) || !Directory.EnumerateFileSystemEntries(model.Directory).Any()) && string.IsNullOrEmpty(zipFilePath))
                throw new FileNotFoundException($"Neuz.App 目录不存在或为空目录，请选择软件包\r\n{model.Directory}");

            // 选择软件包
            if (!string.IsNullOrEmpty(zipFilePath))
            {
                // 备份软件包
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

                // 备份现有NeuzApp
                if (!Directory.Exists(model.BackupDirectory))
                {
                    Log.Information($"create directory:{model.BackupDirectory}");
                    Directory.CreateDirectory(model.BackupDirectory);
                }

                if (Directory.Exists(model.Directory) && Directory.EnumerateFileSystemEntries(model.Directory).Any())
                {
                    var appBkFileName = $"App.{DateTime.Now:yyyyMMddhhmmss}.zip";
                    var appBkFilePath = Path.Combine(model.BackupDirectory, appBkFileName);
                    await FileUtils.ZipFromDirectory(model.Directory, appBkFilePath);
                }

                // 解压zip
                await FileUtils.ZipExtractToDirectory(zipFilePath, model.Directory);

                // 获取API版本号
                var split = zipFileInfo.Name.Split(".zip")[0].Split("_");
                model.Api.Version = split.Length > 1 ? split[1] : split[0];

            }


            // 创建目录
            if (!Directory.Exists(model.Api.ServiceDirectory))
            {
                Log.Information($"create directory:{model.Api.ServiceDirectory}");
                Directory.CreateDirectory(model.Api.ServiceDirectory);
            }

            if (!Directory.Exists(model.Api.LogDirectory))
            {
                Log.Information($"create directory:{model.Api.LogDirectory}");
                Directory.CreateDirectory(model.Api.LogDirectory);
            }


            // 测试数据库连接
            Log.Information($"connect to mysql {model.Api.Database.Host}:{model.Api.Database.Port}");
            var builder = model.GetMySqlConnectionStringBuilder();
            builder.ConnectionTimeout = 10;
            if (!await MySQLUtils.TestConnect(builder.ToString())) throw new ApplicationException("MySQL连接失败");

            // 执行创建数据库脚本
            var sqlPath = Path.Combine(model.SqlDirectory, "create_db.sql");
            var sqlStr  = model.Api.Database.CreateDatabaseSql();
            Log.Information($"create file:{sqlPath}");
            await FileUtils.WriteToFile(sqlStr, sqlPath);

            Log.Information($"import sql file:{sqlPath}");
            builder = model.GetMySqlConnectionStringBuilder();
            await MySQLUtils.ImportSQL(builder.ToString(), filePaths: sqlPath);

            var fileObjs = Directory.GetFiles(model.SqlDirectory, "*.sql")
                                    .Where(f => !f.Equals(sqlPath))
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
                                    .Where(f => f != null)
                                    .ToArray();

            // neuz
            var neuzFilePaths = fileObjs.Where(f => f?.dbName.Equals("neuz", StringComparison.OrdinalIgnoreCase) ?? false)
                                        .OrderBy(f => f?.order ?? "")
                                        .Select(f => f?.orgin ?? "")
                                        .Where(f => !string.IsNullOrEmpty(f))
                                        .ToArray();
            builder          = model.GetMySqlConnectionStringBuilder();
            builder.Database = model.Api.Database.NeuzDbName;

            await MySQLUtils.ImportSQL(builder.ToString(), filePaths: neuzFilePaths,
                currentFileHandler: s => Log.Information($"Import sql file:{s}"),
                importProgressChangedHandler: (o, args) => Log.Information($"{args.PercentageCompleted}%"));


            // neuz_saas
            var neuzSaaSFilePaths = fileObjs.Where(f => f?.dbName.Equals("neuz_saas", StringComparison.OrdinalIgnoreCase) ?? false)
                                            .OrderBy(f => f?.order ?? "")
                                            .Select(f => f?.orgin ?? "")
                                            .Where(f => !string.IsNullOrEmpty(f))
                                            .ToArray();
            builder          = model.GetMySqlConnectionStringBuilder();
            builder.Database = model.Api.Database.NeuzSaaSDbName;

            await MySQLUtils.ImportSQL(builder.ToString(), filePaths: neuzSaaSFilePaths,
                currentFileHandler: s => Log.Information($"Import sql file:{s}"),
                importProgressChangedHandler: (o, args) => Log.Information($"{args.PercentageCompleted}%"));

            // neuz_log
            var neuzLogFilePaths = fileObjs.Where(f => f?.dbName.Equals("neuz_log", StringComparison.OrdinalIgnoreCase) ?? false)
                                           .OrderBy(f => f?.order ?? "")
                                           .Select(f => f?.orgin ?? "")
                                           .Where(f => !string.IsNullOrEmpty(f))
                                           .ToArray();
            builder          = model.GetMySqlConnectionStringBuilder();
            builder.Database = model.Api.Database.NeuzSaaSDbName;

            await MySQLUtils.ImportSQL(builder.ToString(), filePaths: neuzLogFilePaths,
                currentFileHandler: s => Log.Information($"Import sql file:{s}"),
                importProgressChangedHandler: (o, args) => Log.Information($"{args.PercentageCompleted}%"));

            // 备份配置文件
            Log.Information($"backup file: {model.Api.ConfigFilePath}");
            await FileUtils.BackupFile(model.Api.ConfigFilePath);

            // 写入新配置文件
            Log.Information($"create file: {model.Api.ConfigFilePath}");
            var text = model.Api.Database.DbSettings.ToString();
            await FileUtils.WriteToFile(text, model.Api.ConfigFilePath!);

            // 创建windows服务
            // todo 这里没放https
            // var binPath = @$"{model.BinPath} --urls ""http://*:8001;https://*:8002""";
            var binPath = @$"{model.Api.BinPath} --urls ""http://*:{model.Api.Port}""";
            await WinServiceUtils.CreateService(binPath, model.Api.ServiceName, model.Api.ServiceDescription ?? string.Empty, model.Api.ServiceDescription ?? string.Empty);
            Log.Information($"windows 服务创建成功: {model.Api.ServiceName}");

            // 写入ins.conf
            await FileUtils.WriteToFile(model.Api, model.Api.InsConfFilePath);

            return true;
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return false;
        }
    }

    public static async Task<bool> UnInstall(this NeuzAppServiceModel model)
    {
        ArgumentNullException.ThrowIfNull(model.Api.ServiceName, nameof(model.Api.ServiceName));
        ArgumentNullException.ThrowIfNull(model.Api.LogDirectory, nameof(model.Api.LogDirectory));
        try
        {
            var rs = await WinServiceUtils.DeleteService(model.Api.ServiceName);
            if (!rs) return rs;
            // if (File.Exists(model.Api.InsConfFilePath)) File.Delete(model.Api.InsConfFilePath);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            return false;
        }
    }

    public static MySqlConnectionStringBuilder GetMySqlConnectionStringBuilder(this NeuzAppServiceModel model)
    {
        return new MySqlConnectionStringBuilder
        {
            Server             = model.Api.Database.Host,
            Port               = Convert.ToUInt32(model.Api.Database.Port),
            UserID             = model.Api.Database.User,
            Password           = model.Api.Database.Password,
            CharacterSet       = "utf8mb4",
            AllowUserVariables = true
        };
    }

    private static string GetVersion(string fileName)
    {
        var split = fileName.Split(".zip")[0].Split("_");
        var rs    = split.Length > 1? split[1]: split[0];
        return "";
    }
}