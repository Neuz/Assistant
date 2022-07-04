using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using Serilog;

// ReSharper disable InconsistentNaming

namespace Assistant.Utils;

public class MySQLUtils
{
    /// <summary>
    /// 测试连接
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <param name="user"></param>
    /// <param name="pwd"></param>
    /// <param name="timeout">秒</param>
    /// <returns></returns>
    public static bool TestConnect(string host, string port, string user, string pwd, int timeout = 10)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server            = host,
            Port              = Convert.ToUInt32(port),
            UserID            = user,
            Password          = pwd,
            ConnectionTimeout = Convert.ToUInt32(timeout)
        };
        return TestConnect(builder.ToString());
    }

    public static bool TestConnect(string connStr)
    {
        using var conn = new MySqlConnection(connStr);
        conn.Open();
        return conn.State == ConnectionState.Open;
    }

    public static void ImportSQL(string connStr, string filePath, string? errorFilePath = null, Action<object, ImportProgressArgs>? action = null)
    {
        ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));
        ArgumentNullException.ThrowIfNull(connStr, nameof(connStr));

        using var conn = new MySqlConnection(connStr);
        using var cmd  = new MySqlCommand();
        using var mb   = new MySqlBackup(cmd);
        cmd.Connection = conn;
        conn.Open();
        if (!string.IsNullOrEmpty(errorFilePath)) mb.ImportInfo.ErrorLogFile = errorFilePath;

        if (action != null) mb.ImportProgressChanged += action.Invoke;

        mb.ImportFromFile(filePath);

        conn.Close();
    }

    public static Task ImportSQL(string connStr, string[] filePaths, string? errorFilePath = null, 
        Action<string>? currentFileHandler = null,
        Action<object, ImportProgressArgs>? importProgressChangedHandler = null)
    {
        ArgumentNullException.ThrowIfNull(connStr, nameof(connStr));

        return Task.Run(() =>
        {
            using var conn = new MySqlConnection(connStr);
            using var cmd  = new MySqlCommand();
            using var mb   = new MySqlBackup(cmd);
            cmd.Connection = conn;
            conn.Open();
            if (!string.IsNullOrEmpty(errorFilePath)) mb.ImportInfo.ErrorLogFile = errorFilePath;

            if (importProgressChangedHandler != null) mb.ImportProgressChanged += importProgressChangedHandler.Invoke;

            foreach (var filePath in filePaths)
            {
                currentFileHandler?.Invoke(filePath);
                mb.ImportFromFile(filePath);
            }

            conn.Close();
        });
    }
}