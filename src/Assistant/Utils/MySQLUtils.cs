using MySqlConnector;
using System;
using System.Data;
using System.Threading.Tasks;

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
    public static async Task<bool> TestConnect(string host, string port, string user, string pwd, int timeout = 10)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server            = host,
            Port              = Convert.ToUInt32(port),
            UserID            = user,
            Password          = pwd,
            ConnectionTimeout = Convert.ToUInt32(timeout)
        };
        return await TestConnect(builder.ToString());
    }

    public static async Task<bool> TestConnect(string connStr)
    {
        return await Task.Run(() =>
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();
            return conn.State == ConnectionState.Open;
        });
    }

    public static Task ImportSQL(string connStr, string filePath, Action<object, ImportProgressArgs>? importProgressChangedHandler = null)
    {
        ArgumentNullException.ThrowIfNull(connStr, nameof(connStr));

        return Task.Run(() =>
        {
            using var conn = new MySqlConnection(connStr);
            using var cmd  = new MySqlCommand();
            using var mb   = new MySqlBackup(cmd);
            cmd.Connection = conn;
            conn.Open();
            mb.ImportInfo.IntervalForProgressReport = 2000; // 引发 ImportProgressChanged 事件的时间间隔（以毫秒为单位）
            if (importProgressChangedHandler != null) mb.ImportProgressChanged += importProgressChangedHandler.Invoke;
            mb.ImportFromFile(filePath);
            conn.Close();
        });
    }
}