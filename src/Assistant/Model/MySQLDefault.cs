using System.IO;

// ReSharper disable InconsistentNaming

namespace Assistant.Model;

public class MySQLDefault
{
    public string ServiceName => "Neuz.MySQL";
    public string DisplayName => "Neuz.MySQL 服务";
    public string Description => "Neuz.MySQL 数据服务";
    public string BaseDir => Path.Combine("Services", "MySQL");
    public string BinPath => Path.Combine(BaseDir, "bin", "mysqld.exe");
    public string ConfigPath => Path.Combine(BaseDir, "my.ini");
    public int Port => 10001;
    public string User => "root";
    public string Password => "Neuz123";

    public string GetWinSvcBinPath(string globalBasePath)
    {
        var a = Path.Combine(globalBasePath, BinPath);
        var b = Path.Combine(globalBasePath, ConfigPath);
        return @$"""{a}"" --defaults-file=""{b}"" {ServiceName}";
    }
}