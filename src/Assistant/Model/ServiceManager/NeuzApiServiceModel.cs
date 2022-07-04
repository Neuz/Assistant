using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public partial class NeuzApiServiceModel : ServiceBaseModel
{
    public NeuzApiServiceModel()
    {
        var baseDir = Path.Combine(Global.CurrentDir, "App", "api");
        DisplayName        = "Neuz.API";
        Version            = "";
        ServiceName        = "Neuz.API";
        BinPath            = Path.Combine(baseDir, "Neuz.Web.Entry.exe");
        ServiceDescription = "Neuz.API 服务";
        ServiceDirectory   = baseDir;
        LogDirectory       = Path.Combine(baseDir, "logs");
        ConfigFilePath     = Path.Combine(baseDir, "dbsettings.json");
        Installed          = false;
        RunningStatus      = RunningStatus.UnKnown;
        Port               = 5000;
        MySQLHost          = "127.0.0.1";
        MySQLPort          = 10001;
        MySQLUser          = "root";
        MySQLPassword      = "Neuz123";
        DbPrefix           = "";
    }
}

public partial class NeuzApiServiceModel : ServiceBaseModel
{
    public int Port { get; set; }
    public string MySQLHost { get; set; }
    public int MySQLPort { get; set; }
    public string MySQLUser { get; set; }
    public string MySQLPassword { get; set; }

    public string DbPrefix { get; set; }

    public DbSettingModel DbSettings => new()
    {
        ConnectionStrings = new DbSettingModel.ConnectionStringsModel
        {
            DefaultConnection     = string.IsNullOrEmpty(DbPrefix) ? BuildConnString("neuz") : BuildConnString($"{DbPrefix}_neuz"),
            MultiTenantConnection = string.IsNullOrEmpty(DbPrefix) ? BuildConnString("neuz_saas") : BuildConnString($"{DbPrefix}_neuz_saas"),
            LogConnection         = string.IsNullOrEmpty(DbPrefix) ? BuildConnString("neuz_log") : BuildConnString($"{DbPrefix}_neuz_log")
        }
    };

    public string GetDbSettingsSerializer()
    {
        return JsonSerializer.Serialize(DbSettings, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
        });
    }

    private string _connStringTemplate = @"Data Source={0};Database={1};User ID={2};Password={3};pooling=true;port={4};sslmode=none;CharSet=utf8;allowPublicKeyRetrieval=true";

    public string BuildConnString(string dbName)
    {
        return string.Format(_connStringTemplate, MySQLHost, dbName, MySQLUser, MySQLPassword, MySQLPort);
    }

    public string GetCreateDbSQL()
    {
        var _sqlTemplate = @"CREATE DATABASE IF NOT EXISTS {0}neuz CHARACTER SET utf8mb4;
CREATE DATABASE IF NOT EXISTS {0}neuz_saas CHARACTER SET utf8mb4;
CREATE DATABASE IF NOT EXISTS {0}neuz_log CHARACTER SET utf8mb4;";

        return string.IsNullOrEmpty(DbPrefix)
                   ? string.Format(_sqlTemplate, DbPrefix)
                   : string.Format(_sqlTemplate, $"{DbPrefix}_");
    }

    public class DbSettingModel
    {
        public ConnectionStringsModel ConnectionStrings { get; set; }

        public class ConnectionStringsModel
        {
            public string DefaultConnection { get; set; }
            public string MultiTenantConnection { get; set; }
            public string LogConnection { get; set; }
        }
    }
}