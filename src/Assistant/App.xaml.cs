using Assistant.View;
using Syncfusion.SfSkinManager;
using System.Windows;

namespace Assistant;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjY0NTUzQDMyMzAyZTMyMmUzMERWMkNqUDM3VFh0T1hmNlduNHZiTFVEMlZTL01CUlZYV3NyeHBPalc0eGM9");
        SfSkinManager.ApplyStylesOnApplication = true;
        base.OnStartup(e);
        new MainView().Show();
    }
}