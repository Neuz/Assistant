using Assistant.View;
using Syncfusion.SfSkinManager;
using System.Windows;

namespace Assistant;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("ODI3MzQ2QDMyMzAyZTM0MmUzMGRRVlNTUUdVWTRCSk1EV3VxczRNcEE1akV4bTlXcSsxaTJLYUtwT0VmQTg9");
        SfSkinManager.ApplyStylesOnApplication = true;
        base.OnStartup(e);
        new MainView().Show();
    }
}