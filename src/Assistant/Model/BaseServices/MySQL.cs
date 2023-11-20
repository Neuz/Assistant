// ReSharper disable InconsistentNaming

using Assistant.Model.OperationsTools;
using System;
using System.IO;
using Syncfusion.Windows.Shared;

namespace Assistant.Model.BaseServices;

public class MySQL
{
    public string ServiceName { get; set; } = "Neuz.MySQL";

    public WinSvc? WinSvc { get; set; } = null;

    public int Port { get; set; } = 10001;


    public string? BasePath
    {
        get
        {
            var binPath = WinSvc?.BinPath;
            if (binPath == null) return null;
            var index = binPath.IndexOf(" --defaults-file=", StringComparison.Ordinal);
            var path  = index > 0 ? binPath.Remove(index) : binPath;
            return Path.GetDirectoryName(path);
        }
    }

    public string LogPath => BasePath.IsNullOrWhiteSpace()
                                 ? string.Empty
                                 : Path.Combine(BasePath ?? "", "logs", "error.log");

    public string ConfigPath => BasePath.IsNullOrWhiteSpace()
                                    ? string.Empty
                                    : Path.Combine(BasePath ?? "", "my.ini");

    public string DataDir => BasePath.IsNullOrWhiteSpace()
                                 ? string.Empty
                                 : Path.Combine(BasePath ?? "", "data");

    public string TempDir => BasePath.IsNullOrWhiteSpace()
                                 ? string.Empty
                                 : Path.Combine(BasePath ?? "", "tmp");
}