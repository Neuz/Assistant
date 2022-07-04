using Assistant.Model.ServiceManager;
using Assistant.View.WizardControl;
using Assistant.ViewModel.WizardControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

// ReSharper disable InconsistentNaming

namespace Assistant.ViewModel;

public class ServiceManagerViewModel : ObservableObject
{
    public ServiceManagerViewModel()
    {
        FlushCommand               = new AsyncRelayCommand(FlushCommandHandler);
        OpenDirectoryCommand       = new AsyncRelayCommand<object>(OpenDirectoryCommandHandler);
        OpenConfigCommand          = new AsyncRelayCommand<object>(OpenConfigCommandHandler);
        InstallWinServiceCommand   = new AsyncRelayCommand<object>(InstallWinServiceCommandHandler);
        UnInstallWinServiceCommand = new AsyncRelayCommand<object>(UnInstallWinServiceCommandHandler);
        StartWinServiceCommand     = new AsyncRelayCommand<object>(StartWinServiceCommandHandler);
        StopWinServiceCommand      = new AsyncRelayCommand<object>(StopWinServiceCommandHandler);


        _mysql = new MySQLServiceModel();
        _nginx = new NginxServiceModel();
        _redis = new RedisServiceModel();
    }


    #region 属性

    public string Title => "服务管理";

    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    #region Redis

    private RedisServiceModel _redis;

    public RedisServiceModel Redis
    {
        get => _redis;
        set => SetProperty(ref _redis, value);
    }

    #endregion

    #region Nginx

    private NginxServiceModel _nginx;

    public NginxServiceModel Nginx
    {
        get => _nginx;
        set => SetProperty(ref _nginx, value);
    }

    #endregion

    #region MySQL

    private MySQLServiceModel _mysql;

    public MySQLServiceModel MySQL
    {
        get => _mysql;
        set => SetProperty(ref _mysql, value);
    }

    #endregion

    #endregion

    #region 命令

    /// <summary>
    /// 刷新
    /// </summary>
    public ICommand FlushCommand { get; }


    private async Task FlushCommandHandler()
    {
        IsBusy = true;
        Nginx  = await Nginx.Flush();
        MySQL  = await MySQL.Flush();
        Redis  = await Redis.Flush();
        IsBusy = false;
    }

    /// <summary>
    /// 打开目录
    /// </summary>
    public ICommand OpenDirectoryCommand { get; }

    private async Task OpenDirectoryCommandHandler(object? param)
    {
        if (param is ServiceBaseModel model)
        {
            if (!Directory.Exists(model.ServiceDirectory))
            {
                MessageBox.Show("找不到目录");
                return;
            }

            await model.OpenDirectory();
        }
    }

    /// <summary>
    /// 打开配置文件
    /// </summary>
    public ICommand OpenConfigCommand { get; }

    private async Task OpenConfigCommandHandler(object? param)
    {
        if (param is ServiceBaseModel model)
        {
            if (!File.Exists(model.ConfigFilePath))
            {
                MessageBox.Show("找不到目录");
                return;
            }

            await model.OpenConfigFile();
        }
    }

    /// <summary>
    /// 安装服务
    /// </summary>
    public ICommand InstallWinServiceCommand { get; }

    private async Task InstallWinServiceCommandHandler(object? param)
    {
        await FlushCommandHandler();

        IsBusy = true;

        if (param is NginxServiceModel)
        {
            if (Nginx.Installed)
            {
                MessageBox.Show($"[{Nginx.ServiceName}]已安装");
                IsBusy = false;
                return;
            }

            var wizard = new NginxWizardView {DataContext = new NginxWizardViewModel(Nginx)};
            if (!(wizard.ShowDialog() ?? false))
            {
                IsBusy = false;
                return;
            }

            var rs = await Nginx.Install();
            MessageBox.Show(rs ? $"[{Nginx.ServiceName}]安装成功" : $"[{Nginx.ServiceName}]安装失败");
        }

        if (param is MySQLServiceModel)
        {
            if (MySQL.Installed)
            {
                MessageBox.Show($"[{MySQL.ServiceName}]已安装");
                IsBusy = false;
                return;
            }

            var wizard = new MySQLWizardView()
            {
                DataContext = new MySQLWizardViewModel(MySQL)
            };
            if (!(wizard.ShowDialog() ?? false))
            {
                IsBusy = false;
                return;
            }

            var rs = await MySQL.Install();
            MessageBox.Show(rs ? $"[{MySQL.ServiceName}]安装成功" : $"[{MySQL.ServiceName}]安装失败");
        }

        if (param is RedisServiceModel)
        {
            if (Redis.Installed)
            {
                MessageBox.Show($"[{Redis.ServiceName}]已安装");
                IsBusy = false;
                return;
            }

            var wizard = new RedisWizardView()
            {
                DataContext = new RedisWizardViewModel(Redis)
            };
            if (!(wizard.ShowDialog() ?? false))
            {
                IsBusy = false;
                return;
            }

            var rs = await Redis.Install();
            MessageBox.Show(rs ? $"[{Redis.ServiceName}]安装成功" : $"[{Redis.ServiceName}]安装失败");
        }

        IsBusy = false;

        await FlushCommandHandler();
    }

    /// <summary>
    /// 卸载服务
    /// </summary>
    public ICommand UnInstallWinServiceCommand { get; }

    private async Task UnInstallWinServiceCommandHandler(object? param)
    {
        await FlushCommandHandler();
        IsBusy = true;

        if (param is NginxServiceModel)
        {
            if (Nginx.RunningStatus == RunningStatus.Running)
            {
                MessageBox.Show($"[{Nginx.ServiceName}]运行中");
                IsBusy = false;
                return;
            }

            var rs = await Nginx.UnInstall();
            MessageBox.Show(rs ? $"[{Nginx.ServiceName}]卸载成功" : $"[{Nginx.ServiceName}]卸载失败");
        }

        if (param is MySQLServiceModel)
        {
            if (MySQL.RunningStatus == RunningStatus.Running)
            {
                MessageBox.Show($"[{MySQL.ServiceName}]运行中");
                IsBusy = false;
                return;
            }

            var rs = await MySQL.UnInstall();
            MessageBox.Show(rs ? $"[{MySQL.ServiceName}]卸载成功" : $"[{MySQL.ServiceName}]卸载失败");
        }

        if (param is RedisServiceModel)
        {
            if (Redis.RunningStatus == RunningStatus.Running)
            {
                MessageBox.Show($"[{Redis.ServiceName}]运行中");
                IsBusy = false;
                return;
            }

            var rs = await Redis.UnInstall();
            MessageBox.Show(rs ? $"[{Redis.ServiceName}]卸载成功" : $"[{Redis.ServiceName}]卸载失败");
        }

        IsBusy = false;
        await FlushCommandHandler();
    }

    /// <summary>
    /// 启动服务
    /// </summary>
    public ICommand StartWinServiceCommand { get; }

    private async Task StartWinServiceCommandHandler(object? param)
    {
        await FlushCommandHandler();
        IsBusy = true;

        if (param is ServiceBaseModel model)
        {
            if (!Directory.Exists(model.ServiceDirectory))
            {
                MessageBox.Show("找不到目录");
                return;
            }

            await model.Start();
        }

        IsBusy = false;
        await FlushCommandHandler();
    }

    /// <summary>
    /// 停止服务
    /// </summary>
    public ICommand StopWinServiceCommand { get; }

    private async Task StopWinServiceCommandHandler(object? param)
    {
        await FlushCommandHandler();
        IsBusy = true;

        if (param is ServiceBaseModel model)
        {
            if (!Directory.Exists(model.ServiceDirectory))
            {
                MessageBox.Show("找不到目录");
                return;
            }

            await model.Stop();
        }

        IsBusy = false;
        await FlushCommandHandler();
    }

    #endregion
}