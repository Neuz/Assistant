using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Model
{
    public class Global
    {
        public static string CurrentDir => Environment.CurrentDirectory;
        public static string SQLDir => Path.Combine(CurrentDir, "App", "sql");

        public static string InstallConfFileName = "ins.conf";
    }
}
