using System;

namespace Assistant.Model.Backup;

public class BackupFile
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.MinValue;
    public DateTime ModifyTime { get; set; } = DateTime.MinValue;
    public string FileSize { get; set; } = string.Empty;
}