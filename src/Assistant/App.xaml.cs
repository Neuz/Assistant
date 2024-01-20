using Assistant.Services;
using Assistant.View;
using Assistant.View.BaseServices;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Syncfusion.SfSkinManager;
using System.Windows;
using Assistant.Model;

namespace Assistant;

public partial class App
{
    public App()
    {
        // 日志配置
        const string outputTemplate = "[{Level:u3}] [{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.File($"{GlobalConfig.GlobalDir}/logs/.log", shared: true, rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate)
                    .CreateLogger();

        var config = new FileService().Load<GlobalConfig>(GlobalConfig.ConfigPath)??new GlobalConfig();

        // 服务依赖
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
               .AddSingleton(config)
               .AddSingleton<WinSvcService>() // 服务
               .AddSingleton<FileService>() 
               .AddSingleton<StatService>() 
               .AddTransient<MainView>()      // View
               .AddTransient<DashboardView>()
               .AddTransient<SettingsView>()
               .AddTransient<RedisView>()
               .AddTransient<CaddyView>()
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