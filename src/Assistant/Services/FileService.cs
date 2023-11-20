using IniParser;
using System.Collections.Generic;

namespace Assistant.Services;

public class FileService
{
    public void WriteIni(string filePath, string section, IDictionary<string, string> pairs)
    {
        var ini  = new FileIniDataParser();
        var data = ini.ReadFile(filePath);

        foreach (var pair in pairs) data[section][pair.Key] = pair.Value;
        ini.WriteFile(filePath, data);
    }
}