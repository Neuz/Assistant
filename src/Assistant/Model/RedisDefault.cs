using System.IO;

namespace Assistant.Model;

public class RedisDefault
{
    public string ServiceName => "Neuz.Redis";
    public string DisplayName => "Neuz.Redis 服务";
    public string Description => "Neuz.Redis 缓存服务";
    public int Port => 10002;
    public string BaseDir => Path.Combine("Services", "Redis");
    public string BinPath => Path.Combine(BaseDir, "redis-server.exe");
    public string ConfigPath => Path.Combine(BaseDir, "redis.conf");

    public string GetWinSvcBinPath(string globalBasePath)
    {
        var a = Path.Combine(globalBasePath, BinPath);
        var b = Path.Combine(globalBasePath, ConfigPath);
        return @$"""{a}"" --service-run ""{b}""";
    }
}