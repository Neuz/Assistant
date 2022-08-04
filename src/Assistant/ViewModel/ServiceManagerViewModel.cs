using Assistant.Model.ServiceManager;
using Assistant.Utils;
using Assistant.View.WizardControl;
using Assistant.ViewModel.WizardControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CliWrap;

// ReSharper disable InconsistentNaming

namespace Assistant.ViewModel;

public class ServiceManagerViewModel : ObservableObject
{
    public ServiceManagerViewModel()
    {
        FlushCommand      = new AsyncRelayCommand(FlushHandler);
        OpenDirCommand    = new AsyncRelayCommand<object>(OpenDirHandler);
        OpenConfigCommand = new AsyncRelayCommand<object>(OpenConfigHandler);
        InstallCommand    = new AsyncRelayCommand<object>(InstallHandler);
        UnInstallCommand  = new AsyncRelayCommand<object>(UnInstallHandler);
        StartCommand      = new AsyncRelayCommand<object>(StartHandler);
        StopCommand       = new AsyncRelayCommand<object>(StopHandler);
        OpenUrlCommand    = new AsyncRelayCommand<object>(OpenUrlHandler);
        OpenWinSvcCommand = new RelayCommand(OpenWinSvcHandler);


        _mysql   = new MySQLServiceModel();
        _nginx   = new NginxServiceModel();
        _redis   = new RedisServiceModel();
        _neuzApp = new NeuzAppServiceModel();
        _kis     = new NeuzAdapterKisServiceModel();
        _wise    = new NeuzAdapterK3WiseServiceModel();
    }


    #region 属性

    public string Title => "服务管理";

    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _busyText = "";

    public string BusyText
    {
        get => _busyText;
        set => SetProperty(ref _busyText, value);
    }


    private RedisServiceModel _redis;

    public RedisServiceModel Redis
    {
        get => _redis;
        set => SetProperty(ref _redis, value);
    }

    private NginxServiceModel _nginx;

    public NginxServiceModel Nginx
    {
        get => _nginx;
        set => SetProperty(ref _nginx, value);
    }

    private MySQLServiceModel _mysql;

    public MySQLServiceModel MySQL
    {
        get => _mysql;
        set => SetProperty(ref _mysql, value);
    }

    private NeuzAppServiceModel _neuzApp;

    public NeuzAppServiceModel NeuzApp
    {
        get => _neuzApp;
        set => SetProperty(ref _neuzApp, value);
    }

    private NeuzAdapterKisServiceModel _kis;

    public NeuzAdapterKisServiceModel Kis
    {
        get => _kis;
        set => SetProperty(ref _kis, value);
    }

    private NeuzAdapterK3WiseServiceModel _wise;

    public NeuzAdapterK3WiseServiceModel Wise
    {
        get => _wise;
        set => SetProperty(ref _wise, value);
    }

    #endregion

    #region 命令

    /// <summary>
    /// 刷新
    /// </summary>
    public ICommand FlushCommand { get; }


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
            ErrMsg(e.Message, true);
        }

        IsBusy = false;
    }

    private void InfoMsg(string msg, bool showMessageBox = false)
    {
        Log.Information(msg);
        BusyText = msg;
        if (showMessageBox) MessageBox.Show(msg);
    }

    private void ErrMsg(string msg, bool showMessageBox = false)
    {
        Log.Error(msg);
        BusyText = $"[ERR]{msg}";
        if (showMessageBox) MessageBox.Show($"[ERR]{msg}");
    }

    private async Task Flush()
    {
        Nginx   = await Nginx.Flush();
        MySQL   = await MySQL.Flush();
        Redis   = await Redis.Flush();
        NeuzApp = await NeuzApp.Flush();
        Kis     = await Kis.Flush();
        Wise    = await Wise.Flush();
    }

    private async Task FlushHandler()
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
    public ICommand OpenDirCommand { get; }

    private async Task OpenDirHandler(object? param)
    {
        await BusyRun(async () =>
        {
            var dir = param?.ToString() ?? throw new ApplicationException("目录为空");
            if (!Directory.Exists(dir)) throw new ApplicationException($"找不到目录 [{dir}]");

            InfoMsg($"打开目录 [{dir}]");
            await FileUtils.OpenDirectory(dir);
        });
    }

    /// <summary>
    /// 打开配置文件
    /// </summary>
    public ICommand OpenConfigCommand { get; }

    private async Task OpenConfigHandler(object? param)
    {
        await BusyRun(async () =>
        {
            var filePath = param?.ToString() ?? throw new ApplicationException("文件路径为空");
            if (!File.Exists(filePath)) throw new ApplicationException($"找不到文件 [{filePath}]");

            InfoMsg($"打开文件 [{filePath}]");
            await FileUtils.OpenFileWithNotepad(filePath);
        });
    }

    /// <summary>
    /// 安装服务
    /// </summary>
    public ICommand InstallCommand { get; }

    private async Task InstallHandler(object? param)
    {
        await BusyRun(async () =>
        {
            await Flush();
            switch (param)
            {
                case NginxServiceModel:
                {
                    if (Nginx.Installed) throw new ApplicationException($"[{Nginx.ServiceName}]已安装");

                    var wizard = new NginxWizardView {DataContext = new NginxWizardViewModel(Nginx)};
                    if (!(wizard.ShowDialog() ?? false)) break;
                    await Nginx.Install(msg => InfoMsg(msg));
                    InfoMsg($"[{Nginx.ServiceName}]安装成功", true);
                    break;
                }
                case MySQLServiceModel:
                {
                    if (MySQL.Installed) throw new ApplicationException($"[{MySQL.ServiceName}]已安装");

                    var wizard = new MySQLWizardView {DataContext = new MySQLWizardViewModel(MySQL)};
                    if (!(wizard.ShowDialog() ?? false)) break;
                    await MySQL.Install(msg => InfoMsg(msg));
                    InfoMsg($"[{MySQL.ServiceName}]安装成功", true);
                    break;
                }
                case RedisServiceModel:
                {
                    if (Redis.Installed) throw new ApplicationException($"[{Redis.ServiceName}]已安装");

                    var wizard = new RedisWizardView {DataContext = new RedisWizardViewModel(Redis)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    await Redis.Install(msg => InfoMsg(msg));
                    InfoMsg($"[{Redis.ServiceName}]安装成功", true);
                    break;
                }
                case NeuzAppServiceModel:
                {
                    if (NeuzApp.Api.Installed) throw new ApplicationException($"[{NeuzApp.Api.ServiceName}]已安装");

                    var wizard = new NeuzAppWizardView {DataContext = new NeuzAppWizardViewModel(NeuzApp)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    await NeuzApp.Install(wizard.TbZipFilePath.Text, msg => InfoMsg(msg));
                    InfoMsg($"[{NeuzApp.DisplayName}]安装成功", true);
                    break;
                }
                case NeuzAdapterKisServiceModel:
                {
                    if (Kis.Installed) throw new ApplicationException($"[{Kis.ServiceName}]已安装");

                    var wizard = new NeuzAdapterKisWizardView() {DataContext = new NeuzAdapterKisWizardViewModel(Kis)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    await Kis.Install(wizard.TbZipFilePath.Text, msg => InfoMsg(msg));
                    InfoMsg($"[{Kis.DisplayName}]安装成功", true);
                    break;
                }
                case NeuzAdapterK3WiseServiceModel:
                {
                    if (Wise.Installed) throw new ApplicationException($"[{Wise.ServiceName}]已安装");

                    var wizard = new NeuzAdapterK3WiseWizardView() {DataContext = new NeuzAdapterK3WiseWizardViewModel(Wise)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    await Wise.Install(wizard.TbZipFilePath.Text, msg => InfoMsg(msg));
                    InfoMsg($"[{Wise.DisplayName}]安装成功", true);
                    break;
                }
            }
        });
    }

    /// <summary>
    /// 卸载服务
    /// </summary>
    public ICommand UnInstallCommand { get; }

    private async Task UnInstallHandler(object? param)
    {
        await BusyRun(async () =>
        {
            await Flush();
            switch (param)
            {
                case NginxServiceModel:
                {
                    if (Nginx.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Nginx.ServiceName}]运行中");

                    await Nginx.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{Nginx.ServiceName}]卸载成功", true);
                    break;
                }
                case MySQLServiceModel:
                {
                    if (MySQL.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{MySQL.ServiceName}]运行中");

                    await MySQL.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{MySQL.ServiceName}]卸载成功", true);
                    break;
                }
                case RedisServiceModel:
                {
                    if (Redis.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Redis.ServiceName}]运行中");
                    await Redis.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{Redis.ServiceName}]卸载成功", true);
                    break;
                }
                case NeuzAppServiceModel:
                {
                    if (NeuzApp.Api.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{NeuzApp.Api.ServiceName}]运行中");
                    await NeuzApp.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{NeuzApp.DisplayName}]卸载成功", true);
                    break;
                }
                case NeuzAdapterKisServiceModel:
                {
                    if (Kis.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Kis.ServiceName}]运行中");

                    await Kis.UnInstall(msg => InfoMsg(msg));
                    InfoMsg($"[{Kis.DisplayName}]卸载成功", true);
                    break;
                }
                case NeuzAdapterK3WiseServiceModel:
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
    public ICommand StartCommand { get; }

    private async Task StartHandler(object? param)
    {
        if (param is not ServiceBaseModel model) return;

        await BusyRun(async () =>
        {
            BusyText = $"正在启动 [{model.ServiceName}]";
            await model.Start();
            BusyText = $"启动完成 [{model.ServiceName}]";
        });
    }

    public ICommand OpenUrlCommand { get; }

    public async Task OpenUrlHandler(object? param)
    {
        await BusyRun(async () =>
        {
            switch (param)
            {
                case NeuzAdapterKisServiceModel kis:
                {
                    var url = $"http://localhost:{kis.Port}/swagger";
                    await FileUtils.OpenUrl(url);
                    InfoMsg($"打开 url: {url}");
                    break;
                }
                case NeuzAdapterK3WiseServiceModel wise:
                {
                    var url = $"http://localhost:{wise.Port}/swagger";
                    await FileUtils.OpenUrl(url);
                    InfoMsg($"打开 url: {url}");
                    break;
                }
                case string url:
                {
                    await FileUtils.OpenUrl(url);
                    InfoMsg($"打开 url: {url}");
                    break;
                }
            }
        });
    }

    /// <summary>
    /// 停止服务
    /// </summary>
    public ICommand StopCommand { get; }

    private async Task StopHandler(object? param)
    {
        if (param is not ServiceBaseModel model) return;

        await BusyRun(async () =>
        {
            BusyText = $"正在停止 [{model.ServiceName}]";
            await model.Stop();
            BusyText = $"停止完成 [{model.ServiceName}]";
        });
    }


    public ICommand OpenWinSvcCommand { get; }

    private void OpenWinSvcHandler()
    {
        _ = FileUtils.OpenWinSvc();
    }

    #endregion
}