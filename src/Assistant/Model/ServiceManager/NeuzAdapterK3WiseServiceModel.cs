using System.IO;

// ReSharper disable IdentifierTypo

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public partial class NeuzAdapterK3WiseServiceModel : ServiceBaseModel
{
    public NeuzAdapterK3WiseServiceModel()
    {
        var baseDir = Path.Combine(Global.CurrentDir, "Adapters", "K3Wise");
        DisplayName        = "Neuz 适配器 for K3Wise";
        Version            = "";
        ServiceName        = "Neuz.Adapter.K3Wise";
        BinPath            = Path.Combine(baseDir, "NeuzWiseAdapter.exe");
        ServiceDescription = "Neuz 适配器 for K3Wise";
        ServiceDirectory   = baseDir;
        LogDirectory       = Path.Combine(baseDir, "Logs");
        ConfigFilePath     = Path.Combine(baseDir, "app.properties");
        Installed          = false;
        RunningStatus      = RunningStatus.UnKnown;
    }
}

public partial class NeuzAdapterK3WiseServiceModel
{
    public int Port { get; set; } = 10006;
    public string BackupDirectory { get; set; } = Path.Combine(Global.BackupsDir, "Adapters_K3Wise");
    public string PackHistoryDirectory { get; set; } = Path.Combine(Global.PackHistoryDir, "Adapters_K3Wise");


    public string ToConfigString()
    {
        return string.Format(_configTemplate, Port);
    }

    private string _configTemplate = @"app.port={0}";
}