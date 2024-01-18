using System;
using System.IO;
using System.Text.Json.Serialization;

namespace Assistant.Model;

public class GlobalConfig
{
    [JsonIgnore]
    public static string GlobalDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Neuz", "Assistant");

    [JsonIgnore]
    public static string ConfigPath = Path.Combine(GlobalDir, ".config");

    public string BasePath { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = "https://api.neuz.dev";

}