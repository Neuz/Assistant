using System.Text;
using Assistant.Services;
using IniParser;
using Path = System.IO.Path;

namespace Assistant.Test;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public async Task TestMethod1()
    {
        var wss = new WinSvcService();

        try
        {
            var rs = wss.Query("AppReadiness2");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [TestMethod]
    public void TestMethod2()
    {
        var myIniPath = "D:\\Neuz\\Services\\MySQL\\my.ini";

        var baseDir = Path.GetDirectoryName(myIniPath);
        var binPath = Path.Combine(baseDir, "bin", "mysqld.exe");
        var tempDir = Path.Combine(baseDir, "tmp");
        var dataDir = Path.Combine(baseDir, "data");
        var logPath = Path.Combine(baseDir, "logs", "error.log");
        var cc = Path.DirectorySeparatorChar;
        var dd = Path.AltDirectorySeparatorChar;


        var ini       = new FileIniDataParser();
        var data      = ini.ReadFile(myIniPath);
        data["mysqld"]["basedir"]   = $"\"{baseDir.Replace(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)}\"";
        data["mysqld"]["datadir"]   = $"\"{dataDir.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}\"";
        data["mysqld"]["tmpdir"]    = $"\"{tempDir.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}\"";
        data["mysqld"]["log-error"] = $"\"{logPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}\"";
        ini.WriteFile(myIniPath, data, new UTF8Encoding(false));

        
    }


    [TestMethod]
    public void TestMethod3()
    {
        var binPath = "D:\\Neuz\\Services\\MySQL\\bin\\mysqld.exe --defaults-file=\"D:\\Neuz\\Services\\MySQL\\my.ini\" Neuz.MySQL";
        // var binPath = "\"C:\\Program Files\\Common Files\\Apple\\Mobile Device Support\\AppleMobileDeviceService.exe\"";

        var flag  = " --defaults-file=";
        var index = binPath.IndexOf(flag, StringComparison.Ordinal);

        var c = index > 0 ? binPath.Remove(index) : binPath;

        var rs = Path.GetDirectoryName(c);

        var ccc = Path.GetFullPath(c);

        var eee = File.Exists(binPath);
    }
}