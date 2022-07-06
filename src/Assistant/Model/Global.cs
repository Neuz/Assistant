using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Model;

public class Global
{
    public static string CurrentDir => Environment.CurrentDirectory;
    public static string TempDir => Path.Combine(CurrentDir, "Temp");
    public static string BackupsDir => Path.Combine(CurrentDir, "Backups");
    public static string PackHistoryDir => Path.Combine(CurrentDir, "PackHistory");

    public static string InstallConfFileName = "ins.conf";
}