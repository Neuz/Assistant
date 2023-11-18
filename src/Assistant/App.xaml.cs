using Assistant.View;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Syncfusion.SfSkinManager;
using System.Windows;
using Assistant.Services;
using Assistant.View.BaseServices;

namespace Assistant;

public partial class App
{
    public App()
    {
        // 日志配置
        const string outputTemplate = "[{Level:u3}] [{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.File("Assistant.log", shared: true, rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate)
                    .CreateLogger();

        // 服务依赖
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
               .AddSingleton<UIService>() // UI 相关服务
               .AddTransient<MainView>()  // View
               .AddTransient<DashboardView>()
               .AddTransient<MySQLView>()
               .BuildServiceProvider());
    }


    protected override void OnStartup(StartupEventArgs e)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjgxODIwMUAzMjMzMmUzMDJlMzBrUUJOVFVsaDRmNnVJVjFYUHkvNTRlUkpHNzAwdFlNK3ZKdE0xNVY5eVpFPQ==");
        SfSkinManager.ApplyStylesOnApplication = true;
        base.OnStartup(e);
        Ioc.Default.GetRequiredService<MainView>().Show();
    }
}