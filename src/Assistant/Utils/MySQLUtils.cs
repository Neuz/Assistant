﻿using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

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
        return await TestConnect(builder);
    }

    public static async Task<bool> TestConnect(MySqlConnectionStringBuilder builder)
    {
        return await TestConnect(builder.ToString());
    }

    /// <summary>
    /// 测试连接
    /// </summary>
    /// <param name="connStr"></param>
    /// <returns></returns>
    public static async Task<bool> TestConnect(string connStr)
    {
        return await Task.Run(() =>
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();
            return conn.State == ConnectionState.Open;
        });
    }

    /// <summary>
    /// 导入SQL
    /// </summary>
    /// <param name="connStr"></param>
    /// <param name="filePath"></param>
    /// <param name="importProgressChangedHandler"></param>
    /// <returns></returns>
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

    public static async Task<List<string>> GetAllDatabases(MySqlConnectionStringBuilder builder)
    {
        var sql = "SELECT SCHEMA_NAME FROM SCHEMATA";
        builder.Database = "information_schema";
        await using var conn = new MySqlConnection(builder.ToString());
        return (await conn.QueryAsync<string>(sql)).ToList();
    }

    public static async Task<List<string>> GetAllTables(MySqlConnectionStringBuilder builder, string database)
    {
        var sql = $"SELECT TABLE_NAME FROM TABLES WHERE TABLE_SCHEMA = '{database}'";
        builder.Database = "information_schema";
        await using var conn = new MySqlConnection(builder.ToString());
        return (await conn.QueryAsync<string>(sql)).ToList();
    }

    public static async Task BackupToFile()
    {
        
    }
}