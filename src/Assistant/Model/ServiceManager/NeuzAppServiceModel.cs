using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Assistant.Model.ServiceManager;

public class NeuzAppServiceModel
{
    public string? DisplayName { get; set; } = "Neuz.App";
    public string? Version => Api.Version;
    public string Directory { get; } = Path.Combine(Global.CurrentDir, "App");
    public string SqlDirectory { get; } = Path.Combine(Global.CurrentDir, "App", "sql");
    public string WebDirectory { get; } = Path.Combine(Global.CurrentDir, "App", "web");
    public string PdaDirectory { get; } = Path.Combine(Global.CurrentDir, "App", "pda");
    public string BackupDirectory { get; set; } = Path.Combine(Global.BackupsDir, "App");
    public string PackHistoryDirectory { get; set; } = Path.Combine(Global.PackHistoryDir, "App");

    public NeuzApiServiceModel Api { get; set; } = new();

    public class NeuzApiServiceModel : ServiceBaseModel
    {
        public NeuzApiServiceModel()
        {
            DisplayName        = "Neuz.Api";
            Version            = "";
            ServiceName        = "Neuz.Api";
            ServiceDescription = "Neuz.API 服务";
            BinPath            = Path.Combine(Global.CurrentDir, "App", "api", "Neuz.Web.Entry.exe");
            ServiceDirectory   = Path.Combine(Global.CurrentDir, "App", "api");
            LogDirectory       = Path.Combine(Global.CurrentDir, "App", "api", "logs");
            ConfigFilePath     = Path.Combine(Global.CurrentDir, "App", "api", "dbsettings.json");
            Installed          = false;
            RunningStatus      = RunningStatus.UnKnown;
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
                        DefaultConnection     = string.Format(ConnStringTemplate, database.Host, neuzDbName, database.User, database.Password, database.Port),
                        MultiTenantConnection = string.Format(ConnStringTemplate, database.Host, neuzSaaSDbName, database.User, database.Password, database.Port),
                        LogConnection         = string.Format(ConnStringTemplate, database.Host, neuzLogDbName, database.User, database.Password, database.Port)
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
                        Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
                    });
                }
            }
        }
    }
}