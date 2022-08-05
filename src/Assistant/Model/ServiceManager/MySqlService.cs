using Assistant.Utils;
using CliWrap;
using CliWrap.Buffered;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace Assistant.Model.ServiceManager;

public partial class MySqlService : ServiceBase
{
    public override async Task Install(Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(BinPath, nameof(BinPath));
        ArgumentNullException.ThrowIfNull(ServiceName, nameof(ServiceName));
        ArgumentNullException.ThrowIfNull(ConfigFilePath, nameof(ConfigFilePath));
        ArgumentNullException.ThrowIfNull(ServiceDirectory, nameof(ServiceDirectory));
        ArgumentNullException.ThrowIfNull(LogDirectory, nameof(LogDirectory));

        if (!File.Exists(MySQLExePath)) throw new ApplicationException($"文件不存在 {MySQLExePath}");
        if (!File.Exists(BinPath)) throw new ApplicationException($"文件不存在 {BinPath}");

        // 创建目录
        if (!Directory.Exists(TempDirectory))
        {
            Directory.CreateDirectory(TempDirectory);
            infoAction?.Invoke($"创建目录 [{TempDirectory}]");
        }

        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
            infoAction?.Invoke($"创建目录 [{LogDirectory}]");
        }

        // 备份配置文件
        if (File.Exists(ConfigFilePath))
        {
            var rs = await FileUtils.BackupFile(ConfigFilePath);
            infoAction?.Invoke($"备份配置文件 [{rs}]");
        }

        // 更新配置文件
        var iniText = GetIniText(true);
        await File.WriteAllTextAsync(ConfigFilePath, iniText, new UTF8Encoding(false));

        // mysql 初始化
        infoAction?.Invoke($"开始执行MySQL初始化 [{BinPath}]");
        var cli = Cli.Wrap(BinPath)
                     .WithArguments(new[] {"--initialize", "--user=mysql", "--console"})
                     .WithValidation(CommandResultValidation.None);
        infoAction?.Invoke(cli.ToString());
        var cliResult = await cli.ExecuteBufferedAsync();
        infoAction?.Invoke(cliResult.StandardOutput);
        infoAction?.Invoke(cliResult.StandardError);


        // 创建windows服务
        var binPath = @$"{BinPath} --defaults-file=""{ConfigFilePath}"" {ServiceName}";
        await WinServiceUtils.CreateService(binPath, ServiceName, ServiceDescription ?? string.Empty, ServiceDescription ?? string.Empty);
        infoAction?.Invoke($"windows 服务创建成功: [{ServiceName}]");

        // 启动服务
        await WinServiceUtils.StartService(ServiceName);
        infoAction?.Invoke($"windows 服务启动成功: [{ServiceName}]");


        // 设置密码
        infoAction?.Invoke("设置密码");
        var sqlText = GetResetSql();
        var sqlPath = Path.Combine(ServiceDirectory, "reset.sql");
        await File.WriteAllTextAsync(sqlPath, sqlText, new UTF8Encoding(false));
        cli = Cli.Wrap("cmd.exe")
                 .WithArguments(new[] {"/c", MySQLExePath, "-h", "localhost", "-P", $"{MySQLConfig.Port}", "<", sqlPath})
                 .WithValidation(CommandResultValidation.ZeroExitCode);
        cliResult = await cli.ExecuteBufferedAsync();
        infoAction?.Invoke(cliResult.StandardOutput);
        infoAction?.Invoke(cliResult.StandardError);

        if (File.Exists(sqlPath)) File.Delete(sqlPath);

        // 写入新配置文件
        iniText = GetIniText();
        await File.WriteAllTextAsync(ConfigFilePath, iniText, new UTF8Encoding(false));

        await WinServiceUtils.StopService(ServiceName);
        infoAction?.Invoke($"windows 服务停止成功: [{ServiceName}]");

        // 写入ins.conf
        var insConfPath = Path.Combine(ServiceDirectory!, Global.InstallConfFileName);
        infoAction?.Invoke($"写入ins.conf: [{insConfPath}]");
        await FileUtils.WriteToFile(this, insConfPath);
    }

    public override async Task UnInstall(Action<string>? infoAction = null)
    {
        ArgumentNullException.ThrowIfNull(ServiceName, nameof(ServiceName));
        ArgumentNullException.ThrowIfNull(LogDirectory, nameof(LogDirectory));

        if (Directory.Exists(TempDirectory))
        {
            Directory.Delete(TempDirectory, true);
            infoAction?.Invoke($"清理临时目录 [{TempDirectory}]");
        }

        if (File.Exists(InsConfFilePath))
        {
            File.Delete(InsConfFilePath);
            infoAction?.Invoke($"删除ins.conf [{InsConfFilePath}]");
        }

        await base.UnInstall(infoAction);
    }
}

public partial class MySqlService
{
    public MySqlService()
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
}