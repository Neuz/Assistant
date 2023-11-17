using Syncfusion.SfSkinManager;
using System.Windows;

namespace Assistant;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjgxODIwMUAzMjMzMmUzMDJlMzBrUUJOVFVsaDRmNnVJVjFYUHkvNTRlUkpHNzAwdFlNK3ZKdE0xNVY5eVpFPQ==");
        SfSkinManager.ApplyStylesOnApplication = true;
        base.OnStartup(e);
        new MainView().Show();
    }
}