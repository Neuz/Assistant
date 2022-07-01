using System;
using System.IO;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace Assistant.Model.ServiceManager;

public partial class MySQLServiceModel : ServiceBaseModel
{
    public MySQLServiceModel()
    {
        var baseDir = Path.Combine(Global.CurrentDir, "Services", "MySQL");

        DisplayName        = "MySQL";
        Version            = "5.7.36";
        ServiceName        = "Neuz.MySQL";
        BinPath            = Path.Combine(baseDir, "bin", "mysqld.exe");
        ServiceDescription = "Neuz.MySQL 服务";
        ServiceDirectory   = baseDir;
        LogDirectory       = Path.Combine(baseDir, "Logs");
        LogFilePath        = Path.Combine(baseDir, "Logs", "error.log");
        Installed          = false;
        RunningStatus      = RunningStatus.UnKnown;
        ConfigFilePath     = Path.Combine(baseDir, "my.ini");
        MySQLExePath       = Path.Combine(baseDir, "bin", "mysql.exe");
        DataDirectory      = Path.Combine(baseDir, "data");
        TempDirectory      = Path.Combine(baseDir, "tmp");
        MySQLConfig        = new MySQLConfigModel();
    }
}

public partial class MySQLServiceModel : ServiceBaseModel
{
    /// <summary>
    /// mysqld.exe 路径
    /// </summary>
    public string MySQLExePath { get; set; }

    /// <summary>
    /// data 目录
    /// </summary>
    public string DataDirectory { get; set; }

    /// <summary>
    /// tmp 目录
    /// </summary>
    public string TempDirectory { get; set; }

    /// <summary>
    /// log 文件路径
    /// </summary>
    public string LogFilePath { get; set; }


    public MySQLConfigModel MySQLConfig { get; set; }

    public class MySQLConfigModel
    {
        public int Port { get; set; } = 10001;

        public string User { get; set; } = "root";

        public string Password { get; set; } = "Neuz123";

        internal string _myIniText = @"[mysqld] 
port={0}
basedir=""{1}"" 
datadir=""{2}"" 
tmpdir=""{3}"" 
log-error=""{4}"" 
character_set_server=utf8 
default-storage-engine=InnoDB 
lower_case_table_names=1 
max_connections=32000 
explicit_defaults_for_timestamp=1 
skip-name-resolve=1 
innodb_flush_method=normal 
";

        internal string _resetSql = @"
set global read_only=0; 
flush privileges; 
GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY '{0}' WITH GRANT OPTION; 
set global read_only=0; 
flush privileges; ";
    }

    internal string _skipGrantIni = "skip-grant-tables=1 ";

    public string GetIniText(bool skipGrant = false)
    {
        ArgumentNullException.ThrowIfNull(ServiceDirectory, nameof(ServiceDirectory));
        var rs = string.Format(MySQLConfig._myIniText,
            MySQLConfig.Port,
            PathReplace(ServiceDirectory),
            PathReplace(DataDirectory),
            PathReplace(TempDirectory),
            PathReplace(LogFilePath));

        return skipGrant ? $"{rs}\r\n{_skipGrantIni}" : rs;
    }

    private static string PathReplace(string s)
    {
        return s.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public string GetResetSql()
    {
        return string.Format(MySQLConfig._resetSql, MySQLConfig.Password);
    }

    public string GetSkipGrantText()
    {
        return _skipGrantIni;
    }
}