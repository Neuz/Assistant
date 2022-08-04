using Assistant.Utils;
using MySqlConnector;
using Serilog;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

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

    public static async Task Install(this NeuzAppServiceModel model, string? zipFilePath, Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(model.Api.ServiceDirectory, nameof(model.Api.ServiceDirectory));
        ArgumentNullException.ThrowIfNull(model.Api.LogDirectory, nameof(model.Api.LogDirectory));
        ArgumentNullException.ThrowIfNull(model.Api.ServiceName, nameof(model.Api.ServiceName));
        ArgumentNullException.ThrowIfNull(model.Api.ConfigFilePath, nameof(model.Api.ConfigFilePath));
        ArgumentNullException.ThrowIfNull(model.Directory, nameof(model.Directory));


        if ((!Directory.Exists(model.Directory) || !Directory.EnumerateFileSystemEntries(model.Directory).Any()) && string.IsNullOrEmpty(zipFilePath))
            throw new FileNotFoundException($"目录不存在或为空目录，请选择软件包进行安装 \r\n[{model.Directory}]");

        // 备份当前NeuzApp
        if (Directory.Exists(model.Directory))
        {
            infoAction?.Invoke($"创建目录 [{model.BackupDirectory}]");
            Directory.CreateDirectory(model.BackupDirectory);
            var appBkFilePath = Path.Combine(model.BackupDirectory, $"App.{DateTime.Now:yyyyMMddhhmmss}.zip");
            await FileUtils.ZipFromDirectory(model.Directory, appBkFilePath);
            infoAction?.Invoke($"备份当前 NeuzApp 完成 [{appBkFilePath}]");
        }

        infoAction?.Invoke($"创建目录 [{model.Directory}]");
        Directory.CreateDirectory(model.Directory);

        // 更新文件
        if (!string.IsNullOrEmpty(zipFilePath) && File.Exists(zipFilePath))
        {
            infoAction?.Invoke($"开始解压");
            ZipFile.ExtractToDirectory(zipFilePath, model.Directory, true);
            infoAction?.Invoke($"解压完成");
        }

        // 检查api目录是否存在
        if (!Directory.Exists(model.Api.ServiceDirectory)) throw new ApplicationException($"目录不存在 [{model.Api.ServiceDirectory}]");


        // 测试数据库连接
        infoAction?.Invoke($"MySQL数据库连接 {model.Api.Database.Host}:{model.Api.Database.Port}");
        var builder = model.GetMySqlConnectionStringBuilder();
        builder.ConnectionTimeout = 10;
        if (!await MySQLUtils.TestConnect(builder.ToString())) throw new ApplicationException("MySQL连接失败");
        infoAction?.Invoke("MySQL数据库连接 成功");


        // 创建数据库
        infoAction?.Invoke("创建数据库");
        var sqlPath = Path.Combine(model.SqlDirectory, "create_db.sql");
        var sqlStr  = model.Api.Database.CreateDatabaseSql();
        await FileUtils.WriteToFile(sqlStr, sqlPath);
        infoAction?.Invoke($"创建临时脚本 [{sqlPath}]");
        builder = model.GetMySqlConnectionStringBuilder();
        await MySQLUtils.ImportSQL(builder.ToString(), sqlPath, (_, args) => infoAction?.Invoke($"正在执行 {args.PercentageCompleted}% {sqlPath}"));
        File.Delete(sqlPath);
        infoAction?.Invoke($"删除临时脚本 [{sqlPath}]");

        var sqlFiles = Directory.GetFiles(model.SqlDirectory, "*.sql", SearchOption.AllDirectories)
                                .Select(f =>
                                 {
                                     var fi    = new FileInfo(f);
                                     var split = fi.Name[..fi.Name.LastIndexOf(fi.Extension, StringComparison.Ordinal)].Split("_Script_");
                                     return new
                                     {
                                         fileName = fi.Name,
                                         dbName   = (split.Length > 1 ? split[1] : "").ToLower(),
                                         filePath = f
                                     };
                                 })
                                .Where(f => new[] {"neuz", "neuz_log", "neuz_saas"}.Contains(f.dbName))
                                .OrderBy(f => f.dbName)
                                .ThenBy(f => f.fileName)
                                .ToList();
        var sqlIndex = 1;
        foreach (var sqlFile in sqlFiles)
        {
            builder = model.GetMySqlConnectionStringBuilder();
            if (sqlFile.dbName == "neuz") builder.Database      = model.Api.Database.NeuzDbName;
            if (sqlFile.dbName == "neuz_log") builder.Database  = model.Api.Database.NeuzLogDbName;
            if (sqlFile.dbName == "neuz_saas") builder.Database = model.Api.Database.NeuzSaaSDbName;

            try
            {
                await MySQLUtils.ImportSQL(builder.ToString(), sqlFile.filePath,
                    (_, args) =>
                    {
                        infoAction?.Invoke($"[{sqlIndex}/{sqlFiles.Count}] [{args.PercentageCompleted}%] 正在执行 [{builder.Database}] < [{sqlFile.fileName}]");
                    });
            }
            catch (Exception e)
            {
                infoAction?.Invoke($"[ERR] SQL执行失败 [{sqlFile.fileName}]");
            }
            
            sqlIndex++;
        }

        // 备份配置文件
        if (File.Exists(model.Api.ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(model.Api.ConfigFilePath);
            infoAction?.Invoke($"备份配置文件 [{rs}]");
        }

        // 更新配置文件
        var text = model.Api.Database.DbSettings.ToString();
        await FileUtils.WriteToFile(text, model.Api.ConfigFilePath);
        infoAction?.Invoke($"更新配置文件 [{model.Api.ConfigFilePath}]");

        // 创建windows服务
        // todo 这里没放https
        // var binPath = @$"{model.BinPath} --urls ""http://*:8001;https://*:8002""";
        var binPath = @$"{model.Api.BinPath} --urls ""http://*:{model.Api.Port}""";
        await WinServiceUtils.CreateService(binPath, model.Api.ServiceName, model.Api.ServiceDescription ?? string.Empty, model.Api.ServiceDescription ?? string.Empty);
        infoAction?.Invoke($"创建windows服务: {model.Api.ServiceName}");

        // 写入ins.conf
        await FileUtils.WriteToFile(model.Api, model.Api.InsConfFilePath);
        infoAction?.Invoke($"写入ins.conf: [{model.Api.InsConfFilePath}]");
    }

    public static async Task UnInstall(this NeuzAppServiceModel model, Action<string>? infoAction=null)
    {
        ArgumentNullException.ThrowIfNull(model.Api.ServiceName, nameof(model.Api.ServiceName));
        ArgumentNullException.ThrowIfNull(model.Api.LogDirectory, nameof(model.Api.LogDirectory));
        
        
        infoAction?.Invoke($"删除Windows服务 [{model.Api.ServiceName}]");
        await WinServiceUtils.DeleteService(model.Api.ServiceName);

        if (File.Exists(model.Api.InsConfFilePath))
        {
            File.Delete(model.Api.InsConfFilePath);
            infoAction?.Invoke($"删除ins.conf [{model.Api.InsConfFilePath}]");
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
}