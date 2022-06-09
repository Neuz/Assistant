using Assistant.View;
using System.Windows;
using Syncfusion.SfSkinManager;

namespace Assistant;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjUyNzUxQDMyMzAyZTMxMmUzMEJzM1pmcDZvUSsyTXRGb0dnVzFSb2J5K0lEQmRpb1VSMVNQUU5ZK1dkR1k9");
        base.OnStartup(e);
        var main = new MainView();
        SfSkinManager.SetTheme(main, new Theme("FluentLight"));
        main.Show();
    }
}