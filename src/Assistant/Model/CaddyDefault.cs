using System.IO;

namespace Assistant.Model;

public class CaddyDefault
{
    public string ServiceName => "Neuz.Caddy";
    public string DisplayName => "Neuz.Caddy 服务";
    public string Description => "Neuz.Caddy Web服务";
    public string BaseDir => Path.Combine("Services", "Caddy");
    public string BinPath => Path.Combine(BaseDir, "Caddy.exe");
    public string ConfigPath => Path.Combine(BaseDir, "Caddyfile");

    public string GetWinSvcBinPath(string globalBasePath)
    {
        var a = Path.Combine(globalBasePath, BinPath);
        var b = Path.Combine(globalBasePath, ConfigPath);
        return @$"""{a}"" run --config ""{b}""";
    }
}