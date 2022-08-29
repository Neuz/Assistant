using Assistant.Model.ServiceManager;
using Assistant.Utils;
using Assistant.View.ServiceWizard;
using Assistant.ViewModel.ServiceWizard;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using MySQLWizardViewModel = Assistant.ViewModel.ServiceWizard.MySQLWizardViewModel;

// ReSharper disable InconsistentNaming

namespace Assistant.ViewModel;

public partial class ServiceManagerViewModel : ObservableObject
{
    public ServiceManagerViewModel()
    {
    }


    #region 属性

    public string Title => "服务管理";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string busyText = "";

    [ObservableProperty]
    private RedisService _redis = new();

    [ObservableProperty]
    private NginxService _nginx = new();

    [ObservableProperty]
    private MySqlService _mysql = new();

    [ObservableProperty]
    private NeuzAppService _neuzApp = new();

    [ObservableProperty]
    private KisAdapterService _kis = new();

    [ObservableProperty]
    private K3WiseAdapterService _wise = new();

    #endregion

    #region 命令

    private async Task Flush()
    {
        async Task<T> FlushServiceBase<T>(T model) where T : ServiceBase, new()
        {
            var rs = File.Exists(model.InsConfFilePath)
                         ? await FileUtils.ReadFromConf<T>(model.InsConfFilePath) ?? throw new ApplicationException("刷新时序列化失败")
                         : new T();

            var installed = await WinServiceUtils.IsInstalled(model.ServiceName!);
            var status    = await WinServiceUtils.GetRunningStatus(model.ServiceName!);
            rs.Installed     = installed;
            rs.RunningStatus = status;
            return rs;
        }

        async Task<NeuzAppService> FlushNeuzApp(NeuzAppService model)
        {
            ArgumentNullException.ThrowIfNull(model.Api.ServiceName, nameof(model.Api.ServiceName));

            var rs = FileUtils.DeepClone(model) ?? throw new ApplicationException("刷新时序列化失败");

            rs.Api = File.Exists(model.Api.InsConfFilePath)
                         ? await FileUtils.ReadFromConf<NeuzAppService.NeuzApiService>(model.Api.InsConfFilePath) ?? throw new ApplicationException("刷新时序列化失败")
                         : new NeuzAppService.NeuzApiService();

            var installed = await WinServiceUtils.IsInstalled(model.Api.ServiceName!);
            var status    = await WinServiceUtils.GetRunningStatus(model.Api.ServiceName!);
            rs.Api.Installed     = installed;
            rs.Api.RunningStatus = status;
            return rs;
        }


        Nginx   = await FlushServiceBase(Nginx);
        Mysql   = await FlushServiceBase(Mysql);
        Redis   = await FlushServiceBase(Redis);
        Kis     = await FlushServiceBase(Kis);
        Wise    = await FlushServiceBase(Wise);
        NeuzApp = await FlushNeuzApp(NeuzApp);
    }


    private async Task BusyRun(Func<Task>? action, bool withFlush = true)
    {
        IsBusy = true;

        try
        {
            if (action != null) await action.Invoke();

            if (withFlush) await Flush();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            BusyText = $"[ERR] {e.Message}";
            MessageBox.Show($"[ERR] {e.Message}");
        }

        IsBusy = false;
    }

    private void InfoMsg(string msg, bool showMessageBox = false)
    {
        Log.Information(msg);
        BusyText = msg;
        if (showMessageBox) MessageBox.Show(msg);
    }

    /// <summary>
    /// 刷新UI
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task FlushUI()
    {
        await BusyRun(async () =>
        {
            BusyText = "正在刷新";
            await Flush();
            BusyText = "刷新完成";
        }, false);
    }

    /// <summary>
    /// 打开目录
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task OpenDir(string? dir)
    {
        await BusyRun(async () =>
        {
            Guard.IsNotNullOrEmpty(dir, "目录为空");
            if (!Directory.Exists(dir)) ThrowHelper.ThrowArgumentException($"找不到目录 [{dir}]");
            InfoMsg($"正在打开目录 [{dir}]");
            await FileUtils.OpenDirectory(dir);
        });
    }

    /// <summary>
    /// 打开配置文件
    /// </summary>
    [RelayCommand]
    private async Task OpenConfig(string? filePath)
    {
        await BusyRun(async () =>
        {
            Guard.IsNotNullOrEmpty(filePath, "文件路径为空");
            if (!File.Exists(filePath)) ThrowHelper.ThrowArgumentException($"找不到文件 [{filePath}]");

            InfoMsg($"打开文件 [{filePath}]");
            await FileUtils.OpenFileWithNotepad(filePath);
        });
    }

    /// <summary>
    /// 安装服务
    /// </summary>
    [RelayCommand]
    private async Task Install(object? param)
    {
        await BusyRun(async () =>
        {
            await Flush();
            switch (param)
            {
                case NginxService:
                {
                    if (Nginx.Installed) throw new ApplicationException($"[{Nginx.ServiceName}]已安装");

                    var wizard = new NginxWizardView {DataContext = new NginxWizardViewModel(Nginx)};
                    if (!(wizard.ShowDialog() ?? false)) break;
                    await Nginx.Install(msg => InfoMsg(msg));
                    InfoMsg($"[{Nginx.ServiceName}]安装成功", true);
                    break;
                }
                case MySqlService:
                {
                    if (Mysql.Installed) throw new ApplicationException($"[{Mysql.ServiceName}]已安装");

                    var wizard = new MySQLWizardView {DataContext = new MySQLWizardViewModel{_model = Mysql} };
                    if (!(wizard.ShowDialog() ?? false)) break;
                    await Mysql.Install(msg => InfoMsg(msg));
                    InfoMsg($"[{Mysql.ServiceName}]安装成功", true);
                    break;
                }
                case RedisService:
                {
                    if (Redis.Installed) throw new ApplicationException($"[{Redis.ServiceName}]已安装");

                    var wizard = new RedisWizardView {DataContext = new RedisWizardViewModel(Redis)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    await Redis.Install(msg => InfoMsg(msg));
                    InfoMsg($"[{Redis.ServiceName}]安装成功", true);
                    break;
                }
                case NeuzAppService:
                {
                    // if (NeuzApp.Api.Installed) throw new ApplicationException($"[{NeuzApp.Api.ServiceName}]已安装");

                    var wizard = new NeuzAppWizardView {DataContext = new NeuzAppWizardViewModel(NeuzApp)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    await NeuzApp.Install(wizard.TbZipFilePath.Text, msg => InfoMsg(msg));
                    InfoMsg($"[{NeuzApp.DisplayName}]安装成功", true);
                    break;
                }
                case KisAdapterService:
                {
                    if (Kis.Installed) throw new ApplicationException($"[{Kis.ServiceName}]已安装");

                    var wizard = new NeuzAdapterKisWizardView() {DataContext = new NeuzAdapterKisWizardViewModel(Kis)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    Kis.ZipFilePath = wizard.TbZipFilePath.Text;
                    await Kis.Install(msg => InfoMsg(msg));
                    InfoMsg($"[{Kis.DisplayName}]安装成功", true);
                    break;
                }
                case K3WiseAdapterService:
                {
                    if (Wise.Installed) throw new ApplicationException($"[{Wise.ServiceName}]已安装");

                    var wizard = new NeuzAdapterK3WiseWizardView() {DataContext = new NeuzAdapterK3WiseWizardViewModel(Wise)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    Wise.ZipFilePath = wizard.TbZipFilePath.Text;
                    await Wise.Install(msg => InfoMsg(msg));
                    InfoMsg($"[{Wise.DisplayName}]安装成功", true);
                    break;
                }
            }
        });
    }

    /// <summary>
    /// 卸载服务
    /// </summary>
    [RelayCommand]
    private async Task UnInstall(object? param)
    {
        await BusyRun(async () =>
        {
            await Flush();
            switch (param)
            {
                case NginxService:
                {
                    if (Nginx.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Nginx.ServiceName}]运行中");

                    await Nginx.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{Nginx.ServiceName}]卸载成功", true);
                    break;
                }
                case MySqlService:
                {
                    if (Mysql.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Mysql.ServiceName}]运行中");

                    await Mysql.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{Mysql.ServiceName}]卸载成功", true);
                    break;
                }
                case RedisService:
                {
                    if (Redis.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Redis.ServiceName}]运行中");
                    await Redis.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{Redis.ServiceName}]卸载成功", true);
                    break;
                }
                case NeuzAppService:
                {
                    if (NeuzApp.Api.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{NeuzApp.Api.ServiceName}]运行中");
                    await NeuzApp.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{NeuzApp.DisplayName}]卸载成功", true);
                    break;
                }
                case KisAdapterService:
                {
                    if (Kis.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Kis.ServiceName}]运行中");

                    await Kis.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{Kis.DisplayName}]卸载成功", true);
                    break;
                }
                case K3WiseAdapterService:
                {
                    if (Wise.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Wise.ServiceName}]运行中");

                    await Wise.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{Wise.DisplayName}]卸载成功", true);
                    break;
                }
            }
        });
    }

    /// <summary>
    /// 启动服务
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task Start(object? param)
    {
        if (param is not ServiceBase model) return;

        await BusyRun(async () =>
        {
            BusyText = $"正在启动 [{model.ServiceName}]";
            await model.Start();
            BusyText = $"启动完成 [{model.ServiceName}]";
        });
    }

    /// <summary>
    /// 打开URL
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task OpenUrl(string? url)
    {
        await BusyRun(async () =>
        {
            Guard.IsNotNullOrEmpty(url);
            InfoMsg($"打开 url: {url}");
            await FileUtils.OpenUrl(url);
        });
    }

    /// <summary>
    /// 停止服务
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task Stop(object? param)
    {
        if (param is not ServiceBase model) return;

        await BusyRun(async () =>
        {
            BusyText = $"正在停止 [{model.ServiceName}]";
            await model.Stop();
            BusyText = $"停止完成 [{model.ServiceName}]";
        });
    }

    /// <summary>
    /// 打开Windows服务
    /// </summary>
    [RelayCommand]
    private void OpenWinService()
    {
        _ = FileUtils.OpenWinSvc();
    }

    #endregion
}