using CliWrap;
using CliWrap.Buffered;
using Serilog;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Assistant.Utils;

public class FileUtils
{
    public static T? DeepClone<T>(T obj)
    {
        var tmp = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(tmp);
    }

    public static async Task<T?> ReadFromConf<T>(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath), "文件路径为空");
        var text = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(text);
    }

    public static async Task<string> ReadFromConf(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath), "文件路径为空");
        return await File.ReadAllTextAsync(filePath);
    }

    public static async Task WriteToFile<T>(T? obj, string filePath)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        var text = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
        });
        await WriteToFile(text, filePath);
    }

    public static async Task WriteToFile(string? text, string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath), "文件路径为空");
        if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text), "字符串为空");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, text, new UTF8Encoding(false));
    }

    public static async Task<string> BackupFile(string? filePath)
    {
        return await Task.Run(() =>
        {
            if (!File.Exists(filePath)) throw new ApplicationException("文件不存在");
            var dir = Path.GetDirectoryName(filePath);
            ArgumentNullException.ThrowIfNull(dir);
            var oldFileName = Path.GetFileName(filePath);
            var newFilename = $"{oldFileName}.{DateTime.Now:yyyyMMddhhmmss}.bak";
            var newFilePath = Path.Combine(dir, newFilename);
            File.Copy(filePath, newFilePath, true);
            return newFilePath;
        });
    }

    /// <summary>
    /// 打开目录
    /// </summary>
    /// <param name="dirPath"></param>
    /// <returns></returns>
    public static async Task OpenDirectory(string dirPath)
    {
        try
        {
            var cli = Cli.Wrap("explorer.exe")
                         .WithArguments(new[] {"/e,", dirPath})
                         .WithValidation(CommandResultValidation.None);
            Log.Information(cli.ToString());
            await cli.ExecuteBufferedAsync();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    /// <summary>
    /// 打开文件，使用notepad
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task OpenFileWithNotepad(string path)
    {
        try
        {
            var cli = Cli.Wrap("notepad.exe")
                         .WithArguments(path)
                         .WithValidation(CommandResultValidation.ZeroExitCode);

            Log.Information(cli.ToString());
            await cli.ExecuteBufferedAsync();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    public static async Task ZipFromDirectory(string directory, string destFilePath)
    {
        await Task.Run(() => { ZipFile.CreateFromDirectory(directory, destFilePath); });
    }

    public static async Task ZipExtractToDirectory(string zipFilePath, string destDir, bool overwrite = true)
    {
        await Task.Run(() => { ZipFile.ExtractToDirectory(zipFilePath, destDir, overwrite); });
    }

    public static async Task OpenUrl(string url)
    {
        var cli = Cli.Wrap("cmd.exe")
                     .WithArguments(new[] {"/c", "start", url})
                     .WithValidation(CommandResultValidation.None);
        await cli.ExecuteAsync();
    }

    /// <summary>
    /// 打开系统服务 services.msc
    /// </summary>
    /// <returns></returns>
    public static async Task OpenWinSvc()
    {
        await Cli.Wrap("cmd.exe")
                 .WithArguments(new[] {"/c", "services.msc"})
                 .ExecuteAsync();
    }
}