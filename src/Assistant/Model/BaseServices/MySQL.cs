// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Syncfusion.Licensing;

namespace Assistant.Model.BaseServices;

public class MySQL
{
    public MySQL()
    {
        var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "MySQL");
        Port                = 10001;
        ServiceName         = "Neuz.MySQL";
        ServiceDescription  = "Neuz.MySQL 数据服务";
        BinPath             = Path.Combine(baseDir, "bin", "mysqld.exe");
        BinDir              = Path.Combine(baseDir, "bin");
        LogPath             = Path.Combine(baseDir, "logs", "error.log");
        LogDir              = Path.Combine(baseDir, "logs");
        ConfigPath          = Path.Combine(baseDir, "my.ini");
        ConfigDir           = baseDir;
        DataDir             = Path.Combine(baseDir, "data");
        TempDir             = Path.Combine(baseDir, "tmp");
        Installed           = false;
        WinServiceInstalled = false;
        Status              = RunningStatus.UnKnown;
    }

    public int Port { get; set; }
    public string ServiceName { get; set; }
    public string ServiceDescription { get; set; }
    public string BinPath { get; set; }
    public string BinDir { get; set; }
    public string LogPath { get; set; }
    public string LogDir { get; set; }
    public string ConfigPath { get; set; }
    public string ConfigDir { get; set; }
    public string DataDir { get; set; }
    public string TempDir { get; set; }
    public bool Installed { get; set; }
    public bool WinServiceInstalled { get; set; }

    public RunningStatus Status { get; set; }

    public Dictionary<string, object> Infos => new()
    {
        {"实例安装", Installed},
        {"端口", Port},
        {"服务名", ServiceName},
        {"服务描述", ServiceDescription},
        {"BinPath", BinPath},
        {"LogPath", LogPath},
        {"ConfigPath", ConfigPath},
        {"Data目录", DataDir},
        {"是否注册Windows服务", WinServiceInstalled},
        {"Windows服务运行状态", Status.GetDescription()}
    };

    public enum RunningStatus
    {
        [MyDescription(Text = "停止")]
        Stopped = 0,

        [MyDescription(Text = "运行中")]
        Running = 1,

        [MyDescription(Text = "未知")]
        UnKnown = -1
    }
}