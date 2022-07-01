using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Model
{
    public class Global
    {
        public static string CurrentDir => Environment.CurrentDirectory;

        public static string InstallConfFileName = "ins.conf";
    }
}
