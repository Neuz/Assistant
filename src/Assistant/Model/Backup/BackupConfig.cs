using System;
using System.Collections.Generic;

namespace Assistant.Model.Backup;

public class BackupConfig
{
    public string Name { get; set; }

    public DateTime CreateTime { get; set; }
    public DateTime ModifyTime { get; set; }
    public string Note { get; set; }


    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 10001;
    public string UserID { get; set; } = "root";
    public string Password { get; set; } = "Neuz123";

    public string Database { get; set; }

    /// <summary>
    /// 导出的表的列表。如果没有，则将导出所有表
    /// </summary>
    public List<string> Tables { get; set; } = new();

    /// <summary>
    /// 是否导出存储过程
    /// </summary>
    public bool ExportProcedures { get; set; }
    /// <summary>
    /// 是否应导出存储的函数
    /// </summary>
    public bool ExportFunctions { get; set; }
    /// <summary>
    /// 是否应导出存储的触发器
    /// </summary>
    public bool ExportTriggers { get; set; }
    /// <summary>
    /// 是否应导出存储视图
    /// </summary>
    public bool ExportViews { get; set; }
    /// <summary>
    /// 是否应导出存储的事件
    /// </summary>
    public bool ExportEvents { get; set; }
}