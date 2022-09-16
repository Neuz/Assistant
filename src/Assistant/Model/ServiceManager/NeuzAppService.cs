using Assistant.Utils;
using MySqlConnector;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Assistant.Model.ServiceManager;

public partial class NeuzAppService
{
    public string? DisplayName { get; set; } = "Neuz.App";
    public string? Version => Api.Version;
    public string BaseDirectory { get; } = Path.Combine(Global.CurrentDir, "App");
    public string SqlDirectory { get; } = Path.Combine(Global.CurrentDir, "App", "sql");
    public string WebDirectory { get; } = Path.Combine(Global.CurrentDir, "App", "web");
    public string PdaDirectory { get; } = Path.Combine(Global.CurrentDir, "App", "pda");
    public string BackupDirectory { get; set; } = Path.Combine(Global.BackupsDir, "App");

    public NeuzApiService Api { get; set; } = new();

    public class NeuzApiService : ServiceBase
    {
        public NeuzApiService()
        {
            DisplayName = "Neuz.Api";
            Version = "";
            ServiceName = "Neuz.Api";
            ServiceDescription = "Neuz.API 服务";
            BinPath = Path.Combine(Global.CurrentDir, "App", "api", "Neuz.Web.Entry.exe");
            ServiceDirectory = Path.Combine(Global.CurrentDir, "App", "api");
            LogDirectory = Path.Combine(Global.CurrentDir, "App", "api", "logs");
            ConfigFilePath = Path.Combine(Global.CurrentDir, "App", "api", "dbsettings.json");
            Installed = false;
            RunningStatus = RunningStatus.UnKnown;
        }


        public int Port { get; set; } = 5000;

        public DatabaseModel Database { get; set; } = new();


        public class DatabaseModel
        {
            public string? Host { get; set; } = "localhost";
            public int Port { get; set; } = 10001;
            public string? User { get; set; } = "root";
            public string? Password { get; set; } = "Neuz123";

            public string? DbPrefix { get; set; } = "";

            public string NeuzDbName => string.IsNullOrEmpty(DbPrefix) ? "neuz" : $"{DbPrefix}_neuz";
            public string NeuzSaaSDbName => string.IsNullOrEmpty(DbPrefix) ? "neuz_saas" : $"{DbPrefix}_neuz_saas";
            public string NeuzLogDbName => string.IsNullOrEmpty(DbPrefix) ? "neuz_log" : $"{DbPrefix}_neuz_log";

            public string CreateDatabaseSql()
            {
                return @$"CREATE DATABASE IF NOT EXISTS {NeuzDbName} CHARACTER SET utf8mb4;
CREATE DATABASE IF NOT EXISTS {NeuzSaaSDbName} CHARACTER SET utf8mb4;
CREATE DATABASE IF NOT EXISTS {NeuzLogDbName} CHARACTER SET utf8mb4;";
            }

            public DbSettingsModel DbSettings => new(this, NeuzDbName, NeuzSaaSDbName, NeuzLogDbName);

            public class DbSettingsModel
            {
                public const string ConnStringTemplate = @"Data Source={0};Database={1};User ID={2};Password={3};pooling=true;port={4};sslmode=none;CharSet=utf8;allowPublicKeyRetrieval=true";

                public DbSettingsModel(DatabaseModel database, string neuzDbName, string neuzSaaSDbName, string neuzLogDbName)
                {
                    ConnectionStrings = new ConnectionStringsModel
                    {
                        DefaultConnection = string.Format(ConnStringTemplate, database.Host, neuzDbName, database.User, database.Password, database.Port),
                        MultiTenantConnection = string.Format(ConnStringTemplate, database.Host, neuzSaaSDbName, database.User, database.Password, database.Port),
                        LogConnection = string.Format(ConnStringTemplate, database.Host, neuzLogDbName, database.User, database.Password, database.Port)
                    };
                }

                public ConnectionStringsModel? ConnectionStrings { get; set; }

                public class ConnectionStringsModel
                {
                    public string? DefaultConnection { get; set; }
                    public string? MultiTenantConnection { get; set; }
                    public string? LogConnection { get; set; }
                }

                public override string ToString()
                {
                    return JsonSerializer.Serialize(this, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    });
                }
            }
        }
    }
}

public partial class NeuzAppService
{
    public async Task<NeuzAppService> Flush()
    {
        ArgumentNullException.ThrowIfNull(Api.ServiceName, nameof(Api.ServiceName));

        var rs = FileUtils.DeepClone(this) ?? throw new ApplicationException("刷新时序列化失败");

        rs.Api = File.Exists(Api.InsConfFilePath)
                     ? await FileUtils.ReadFromConf<NeuzApiService>(Api.InsConfFilePath) ?? throw new ApplicationException("刷新时序列化失败")
                     : new NeuzApiService();

        var installed = await WinServiceUtils.IsInstalled(Api.ServiceName!);
        var status = await WinServiceUtils.GetRunningStatus(Api.ServiceName!);
        rs.Api.Installed = installed;
        rs.Api.RunningStatus = status;
        return rs;
    }

    public async Task Install(string? zipFilePath, Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(Api.ServiceDirectory, nameof(Api.ServiceDirectory));
        ArgumentNullException.ThrowIfNull(Api.LogDirectory, nameof(Api.LogDirectory));
        ArgumentNullException.ThrowIfNull(Api.ServiceName, nameof(Api.ServiceName));
        ArgumentNullException.ThrowIfNull(Api.ConfigFilePath, nameof(Api.ConfigFilePath));
        ArgumentNullException.ThrowIfNull(BaseDirectory, nameof(BaseDirectory));


        if ((!Directory.Exists(BaseDirectory) || !Directory.EnumerateFileSystemEntries(BaseDirectory).Any()) && string.IsNullOrEmpty(zipFilePath))
            throw new FileNotFoundException($"目录不存在或为空目录，请选择软件包进行安装 \r\n[{BaseDirectory}]");

        // 备份当前NeuzApp
        if (Directory.Exists(BaseDirectory))
        {
            infoAction?.Invoke($"创建目录 [{BackupDirectory}]");
            Directory.CreateDirectory(BackupDirectory);
            var appBkFilePath = Path.Combine(BackupDirectory, $"App.{DateTime.Now:yyyyMMddhhmmss}.zip");
            //TODO：备份时如何排除log文件
            infoAction?.Invoke($"备份当前 NeuzApp");
            await FileUtils.ZipFromDirectory(BaseDirectory, appBkFilePath);
            infoAction?.Invoke($"备份当前 NeuzApp 完成 [{appBkFilePath}]");
        }

        infoAction?.Invoke($"创建目录 [{BaseDirectory}]");
        Directory.CreateDirectory(BaseDirectory);

        // 更新文件
        if (!string.IsNullOrEmpty(zipFilePath) && File.Exists(zipFilePath))
        {
            infoAction?.Invoke($"开始解压");
            await FileUtils.ZipExtractToDirectory(zipFilePath, BaseDirectory);
            infoAction?.Invoke($"解压完成");
        }

        // 检查api目录是否存在
        if (!Directory.Exists(Api.ServiceDirectory)) throw new ApplicationException($"目录不存在 [{Api.ServiceDirectory}]");


        // 测试数据库连接
        infoAction?.Invoke($"MySQL数据库连接 {Api.Database.Host}:{Api.Database.Port}");
        var builder = GetMySqlConnectionStringBuilder();
        builder.ConnectionTimeout = 10;
        if (!await MySQLUtils.TestConnect(builder.ToString())) throw new ApplicationException("MySQL连接失败");
        infoAction?.Invoke("MySQL数据库连接 成功");


        // 创建数据库
        infoAction?.Invoke("创建数据库");
        var sqlPath = Path.Combine(SqlDirectory, "create_db.sql");
        var sqlStr = Api.Database.CreateDatabaseSql();
        await FileUtils.WriteToFile(sqlStr, sqlPath);
        infoAction?.Invoke($"创建临时脚本 [{sqlPath}]");
        builder = GetMySqlConnectionStringBuilder();
        await MySQLUtils.ImportSQL(builder.ToString(), sqlPath, (_, args) => infoAction?.Invoke($"正在执行 {args.PercentageCompleted}% {sqlPath}"));
        File.Delete(sqlPath);
        infoAction?.Invoke($"删除临时脚本 [{sqlPath}]");

        var sqlFiles = Directory.GetFiles(SqlDirectory, "*.sql", SearchOption.AllDirectories)
                                .Select(f =>
                                 {
                                     var fi = new FileInfo(f);
                                     var split = fi.Name[..fi.Name.LastIndexOf(fi.Extension, StringComparison.Ordinal)].Split("_script_");
                                     return new
                                     {
                                         fileName = fi.Name,
                                         dbName = (split.Length > 1 ? split[1] : "").ToLower(),
                                         filePath = f
                                     };
                                 })
                                .Where(f => new[] { "neuz", "neuz_log", "neuz_saas" }.Contains(f.dbName))
                                .OrderBy(f => f.dbName)
                                .ThenBy(f => f.fileName)
                                .ToList();
        var sqlIndex = 1;
        foreach (var sqlFile in sqlFiles)
        {
            builder = GetMySqlConnectionStringBuilder();
            if (sqlFile.dbName == "neuz") builder.Database = Api.Database.NeuzDbName;
            if (sqlFile.dbName == "neuz_log") builder.Database = Api.Database.NeuzLogDbName;
            if (sqlFile.dbName == "neuz_saas") builder.Database = Api.Database.NeuzSaaSDbName;

            try
            {
                await MySQLUtils.ImportSQL(builder.ToString(), sqlFile.filePath,
                    (_, args) => { infoAction?.Invoke($"[{sqlIndex}/{sqlFiles.Count}] [{args.PercentageCompleted}%] 正在执行 [{builder.Database}] < [{sqlFile.fileName}]"); });
            }
            catch (Exception e)
            {
                infoAction?.Invoke($"[ERR] SQL执行失败 [{sqlFile.fileName}]");
            }

            sqlIndex++;
        }

        //无需单独备份配置文件，因为前面已整个 NeuzApp 备份
        //// 备份配置文件
        //if (File.Exists(Api.ConfigFilePath))
        //{
        //    var rs = await FileUtils.BackupFile(Api.ConfigFilePath);
        //    infoAction?.Invoke($"备份配置文件 [{rs}]");
        //}

        // 更新配置文件
        var text = Api.Database.DbSettings.ToString();
        await FileUtils.WriteToFile(text, Api.ConfigFilePath);
        infoAction?.Invoke($"更新配置文件 [{Api.ConfigFilePath}]");

        // 创建windows服务
        var installed = await WinServiceUtils.IsInstalled(Api.ServiceName!);
        if (!installed)
        {
            // todo 这里没放https
            // var binPath = @$"{model.BinPath} --urls ""http://*:8001;https://*:8002""";
            var binPath = @$"{Api.BinPath} --urls ""http://*:{Api.Port}""";
            await WinServiceUtils.CreateService(binPath, Api.ServiceName, Api.ServiceDescription ?? string.Empty, Api.ServiceDescription ?? string.Empty);
            infoAction?.Invoke($"创建windows服务: {Api.ServiceName}");
        }

        // 写入ins.conf
        await FileUtils.WriteToFile(Api, Api.InsConfFilePath);
        infoAction?.Invoke($"写入ins.conf: [{Api.InsConfFilePath}]");
    }

    public async Task UnInstall(Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(Api.ServiceName, nameof(Api.ServiceName));
        ArgumentNullException.ThrowIfNull(Api.LogDirectory, nameof(Api.LogDirectory));

        if (File.Exists(Api.InsConfFilePath))
        {
            File.Delete(Api.InsConfFilePath);
            infoAction?.Invoke($"删除ins.conf [{Api.InsConfFilePath}]");
        }

        infoAction?.Invoke($"删除Windows服务 [{Api.ServiceName}]");
        await WinServiceUtils.DeleteService(Api.ServiceName);
    }

    private MySqlConnectionStringBuilder GetMySqlConnectionStringBuilder()
    {
        return new MySqlConnectionStringBuilder
        {
            Server = Api.Database.Host,
            Port = Convert.ToUInt32(Api.Database.Port),
            UserID = Api.Database.User,
            Password = Api.Database.Password,
            CharacterSet = "utf8mb4",
            AllowUserVariables = true
        };
    }
}