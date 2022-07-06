using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;

// ReSharper disable IdentifierTypo

namespace Assistant.Model.ServiceManager;

// ReSharper disable once InconsistentNaming
public partial class NeuzAdapterKisServiceModel : ServiceBaseModel
{
    public NeuzAdapterKisServiceModel()
    {
        var baseDir = Path.Combine(Global.CurrentDir, "Adapters", "kisu");
        DisplayName        = "Neuz 适配器 for Kis Ultimate";
        Version            = "";
        ServiceName        = "Neuz.Adapter.KisUltimate";
        BinPath            = Path.Combine(baseDir, "NeuzKisAdapter.exe");
        ServiceDescription = "Neuz 适配器 for Kis Ultimate";
        ServiceDirectory   = baseDir;
        LogDirectory       = Path.Combine(baseDir, "Logs");
        ConfigFilePath     = Path.Combine(baseDir, "app.properties");
        Installed          = false;
        RunningStatus      = RunningStatus.UnKnown;
    }
}

public partial class NeuzAdapterKisServiceModel
{
    public int Port { get; set; } = 10005;
    public string BackupDirectory { get; set; } = Path.Combine(Global.BackupsDir, "Adapters_kisu");
    public string PackHistoryDirectory { get; set; } = Path.Combine(Global.PackHistoryDir, "Adapters_kisu");

    public KingdeeAccountModel KingdeeAccount { get; set; } = new();


    public string ToConfigString()
    {
        return string.Format(_configTemplate, Port, KingdeeAccount.Number, KingdeeAccount.User, KingdeeAccount.Password);
    }

    private string _configTemplate = @"app.port={0}
erp.acctId={1}
erp.acctUserName={2}
erp.acctPassword={3}";

    public class KingdeeAccountModel
    {
        public string Number { get; set; } = "";
        public string User { get; set; } = "administrator";
        public string Password { get; set; } = "";
    }
}