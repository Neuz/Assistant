﻿using System;
using IniParser;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Assistant.Model;
using CommunityToolkit.Diagnostics;

// ReSharper disable InconsistentNaming

namespace Assistant.Services;

public class FileService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    private readonly Encoding _encoding = new UTF8Encoding(false);


    public T? Load<T>(string filePath)
    {
        Guard.IsNotNullOrEmpty(filePath);
        if (!File.Exists(filePath)) return default;
        var text = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<T>(text);
    }

    public void Save<T>(string filePath, T obj)
    {
        Guard.IsNotNullOrEmpty(filePath);
        Guard.IsNotNull(obj);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException());
        var text = JsonSerializer.Serialize(obj, _options);
        File.WriteAllText(filePath, text, _encoding);
    }

    public async Task ZipExtract(string zipFilePath, string destDir, bool overwrite = true)
    {
        await Task.Run(() => ZipFile.ExtractToDirectory(zipFilePath, destDir, overwrite));
    }

    /// <summary>
    /// 初始化my.ini
    /// </summary>
    /// <param name="baseDir"></param>
    public void InitMySqlIni(string baseDir)
    {
        var iniPath = Path.Combine(baseDir, "my.ini");
        var dataDir = Path.Combine(baseDir, "data");
        var tempDir = Path.Combine(baseDir, "tmp");
        var logPath = Path.Combine(baseDir, "logs", "error.log");

        var ini  = new FileIniDataParser();
        var data = ini.ReadFile(iniPath);
        data["mysqld"]["basedir"]   = $"\"{baseDir.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}\"";
        data["mysqld"]["datadir"]   = $"\"{dataDir.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}\"";
        data["mysqld"]["tmpdir"]    = $"\"{tempDir.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}\"";
        data["mysqld"]["log-error"] = $"\"{logPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}\"";
        ini.WriteFile(iniPath, data, new UTF8Encoding(false));
    }
}