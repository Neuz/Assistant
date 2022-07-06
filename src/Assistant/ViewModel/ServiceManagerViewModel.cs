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
        OpenWinSvcCommand = new AsyncRelayCommand<object>(OpenWinSvcHandler);


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

    private string _indicatorText = "";

    public string IndicatorText
    {
        get => _indicatorText;
        set => SetProperty(ref _indicatorText, value);
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


    private async Task FlushHandler()
    {
        IsBusy  = true;
        Nginx   = await Nginx.Flush();
        MySQL   = await MySQL.Flush();
        Redis   = await Redis.Flush();
        NeuzApp = await NeuzApp.Flush();
        Kis     = await Kis.Flush();
        Wise    = await Wise.Flush();
        IsBusy  = false;
    }

    /// <summary>
    /// 打开目录
    /// </summary>
    public ICommand OpenDirCommand { get; }

    private async Task OpenDirHandler(object? param)
    {
        var dir = param?.ToString();
        if (Directory.Exists(dir))
            await FileUtils.OpenDirectory(dir);
        else
            MessageBox.Show($"找不到目录\r\n{dir}");
    }

    /// <summary>
    /// 打开配置文件
    /// </summary>
    public ICommand OpenConfigCommand { get; }

    private async Task OpenConfigHandler(object? param)
    {
        var filePath = param?.ToString();
        if (File.Exists(filePath))
            await FileUtils.OpenFileWithNotepad(filePath);
        else
            MessageBox.Show($"找不到文件\r\n{filePath}]");
    }

    /// <summary>
    /// 安装服务
    /// </summary>
    public ICommand InstallCommand { get; }

    private async Task InstallHandler(object? param)
    {
        await FlushHandler();

        IsBusy = true;

        try
        {
            switch (param)
            {
                case NginxServiceModel:
                {
                    if (Nginx.Installed) throw new ApplicationException($"[{Nginx.ServiceName}]已安装");

                    var wizard = new NginxWizardView {DataContext = new NginxWizardViewModel(Nginx)};
                    if (!(wizard.ShowDialog() ?? false)) break;
                    var rs = await Nginx.Install();
                    MessageBox.Show(rs ? $"[{Nginx.ServiceName}]安装成功" : $"[{Nginx.ServiceName}]安装失败");
                    break;
                }
                case MySQLServiceModel:
                {
                    if (MySQL.Installed) throw new ApplicationException($"[{MySQL.ServiceName}]已安装");

                    var wizard = new MySQLWizardView {DataContext = new MySQLWizardViewModel(MySQL)};
                    if (!(wizard.ShowDialog() ?? false)) break;
                    var rs = await MySQL.Install();
                    MessageBox.Show(rs ? $"[{MySQL.ServiceName}]安装成功" : $"[{MySQL.ServiceName}]安装失败");
                    break;
                }
                case RedisServiceModel:
                {
                    if (Redis.Installed) throw new ApplicationException($"[{Redis.ServiceName}]已安装");

                    var wizard = new RedisWizardView {DataContext = new RedisWizardViewModel(Redis)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    var rs = await Redis.Install();
                    MessageBox.Show(rs ? $"[{Redis.ServiceName}]安装成功" : $"[{Redis.ServiceName}]安装失败");
                    break;
                }
                case NeuzAppServiceModel:
                {
                    if (NeuzApp.Api.Installed) throw new ApplicationException($"[{NeuzApp.Api.ServiceName}]已安装");

                    var wizard = new NeuzAppWizardView {DataContext = new NeuzAppWizardViewModel(NeuzApp)};
                    if (!(wizard.ShowDialog() ?? false)) break;


                    var rs = await NeuzApp.Install(wizard.TbZipFilePath.Text);
                    MessageBox.Show(rs ? $"[{NeuzApp.DisplayName}]安装成功" : $"[{NeuzApp.DisplayName}]安装失败");
                    break;
                }
                case NeuzAdapterKisServiceModel:
                {
                    if (Kis.Installed) throw new ApplicationException($"[{Kis.ServiceName}]已安装");

                    var wizard = new NeuzAdapterKisWizardView() {DataContext = new NeuzAdapterKisWizardViewModel(Kis)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    var rs = await Kis.Install(wizard.TbZipFilePath.Text);
                    MessageBox.Show(rs ? $"[{Kis.DisplayName}]安装成功" : $"[{Kis.DisplayName}]安装失败");
                    break;
                }
                case NeuzAdapterK3WiseServiceModel:
                {
                    if (Wise.Installed) throw new ApplicationException($"[{Wise.ServiceName}]已安装");

                    var wizard = new NeuzAdapterK3WiseWizardView() {DataContext = new NeuzAdapterK3WiseWizardViewModel(Wise)};
                    if (!(wizard.ShowDialog() ?? false)) break;

                    var rs = await Wise.Install(wizard.TbZipFilePath.Text);
                    MessageBox.Show(rs ? $"[{Wise.DisplayName}]安装成功" : $"[{Wise.DisplayName}]安装失败");
                    break;
                }
            }
        }
        catch (ApplicationException aex)
        {
            MessageBox.Show(aex.Message);
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }


        IsBusy = false;

        await FlushHandler();
    }

    /// <summary>
    /// 卸载服务
    /// </summary>
    public ICommand UnInstallCommand { get; }

    private async Task UnInstallHandler(object? param)
    {
        await FlushHandler();
        IsBusy = true;

        try
        {
            switch (param)
            {
                case NginxServiceModel:
                {
                    if (Nginx.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Nginx.ServiceName}]运行中");

                    var rs = await Nginx.UnInstall();
                    MessageBox.Show(rs ? $"[{Nginx.ServiceName}]卸载成功" : $"[{Nginx.ServiceName}]卸载失败");
                    break;
                }
                case MySQLServiceModel:
                {
                    if (MySQL.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{MySQL.ServiceName}]运行中");

                    var rs = await MySQL.UnInstall();
                    MessageBox.Show(rs ? $"[{MySQL.ServiceName}]卸载成功" : $"[{MySQL.ServiceName}]卸载失败");
                    break;
                }
                case RedisServiceModel:
                {
                    if (Redis.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Redis.ServiceName}]运行中");

                    var rs = await Redis.UnInstall();
                    MessageBox.Show(rs ? $"[{Redis.ServiceName}]卸载成功" : $"[{Redis.ServiceName}]卸载失败");
                    break;
                }
                case NeuzAppServiceModel:
                {
                    if (NeuzApp.Api.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{NeuzApp.Api.ServiceName}]运行中");

                    var rs = await NeuzApp.UnInstall();
                    MessageBox.Show(rs ? $"[{NeuzApp.DisplayName}]卸载成功" : $"[{NeuzApp.DisplayName}]卸载失败");
                    break;
                }
                case NeuzAdapterKisServiceModel:
                {
                    if (Kis.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Kis.ServiceName}]运行中");

                    var rs = await Kis.UnInstall();
                    MessageBox.Show(rs ? $"[{Kis.DisplayName}]卸载成功" : $"[{Kis.DisplayName}]卸载失败");
                    break;
                }
                case NeuzAdapterK3WiseServiceModel:
                {
                    if (Wise.RunningStatus == RunningStatus.Running) throw new ApplicationException($"[{Wise.ServiceName}]运行中");

                    var rs = await Wise.UnInstall();
                    MessageBox.Show(rs ? $"[{Wise.DisplayName}]卸载成功" : $"[{Wise.DisplayName}]卸载失败");
                    break;
                }
            }
        }
        catch (ApplicationException aex)
        {
            MessageBox.Show(aex.Message);
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }


        IsBusy = false;
        await FlushHandler();
    }

    /// <summary>
    /// 启动服务
    /// </summary>
    public ICommand StartCommand { get; }

    private async Task StartHandler(object? param)
    {
        await FlushHandler();
        IsBusy = true;

        if (param is ServiceBaseModel model) await model.Start();

        IsBusy = false;
        await FlushHandler();
    }

    public ICommand OpenUrlCommand { get; }

    public async Task OpenUrlHandler(object? param)
    {
        switch (param)
        {
            case NeuzAdapterKisServiceModel kis:
            {
                var kisUrl = $"http://localhost:{kis.Port}/swagger";
                Log.Information($"open url: {kisUrl}");
                await FileUtils.OpenUrl(kisUrl);
                break;
            }
            case NeuzAdapterK3WiseServiceModel wise:
            {
                var wiseUrl = $"http://localhost:{wise.Port}/swagger";
                Log.Information($"open url: {wiseUrl}");
                await FileUtils.OpenUrl(wiseUrl);
                break;
                }
            case string url:
                Log.Information($"open url: {url}");
                await FileUtils.OpenUrl(url);
                break;
        }
    }

    /// <summary>
    /// 停止服务
    /// </summary>
    public ICommand StopCommand { get; }

    private async Task StopHandler(object? param)
    {
        await FlushHandler();
        IsBusy = true;

        if (param is ServiceBaseModel model) await model.Stop();

        IsBusy = false;
        await FlushHandler();
    }


    public ICommand OpenWinSvcCommand { get; }

    private async Task OpenWinSvcHandler(object? param)
    {
        await Cli.Wrap("cmd.exe")
                 .WithArguments(new []{"/c", "services.msc" })
                 .ExecuteAsync();
    }

    #endregion
}