using Assistant.View;
using System.Windows;
using Serilog;
using Syncfusion.SfSkinManager;

namespace Assistant;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjY0NTUzQDMyMzAyZTMyMmUzMERWMkNqUDM3VFh0T1hmNlduNHZiTFVEMlZTL01CUlZYV3NyeHBPalc0eGM9");
        // SfSkinManager.ApplyStylesOnApplication = true;
        base.OnStartup(e);
        var main = new MainView();
        main.Show();
    }
}