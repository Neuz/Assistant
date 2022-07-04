using Assistant.Model.ServiceManager;
using Assistant.View.WizardControl;
using Assistant.ViewModel.WizardControl;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CliWrap;
using CliWrap.Buffered;
using Serilog;

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
        OpenUrl           = new AsyncRelayCommand<object>(OpenUrlHandler);


        _mysql   = new MySQLServiceModel();
        _nginx   = new NginxServiceModel();
        _redis   = new RedisServiceModel();
        _neuzApi = new NeuzApiServiceModel();
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

    private NeuzApiServiceModel _neuzApi;

    public NeuzApiServiceModel NeuzApi
    {
        get => _neuzApi;
        set => SetProperty(ref _neuzApi, value);
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
        NeuzApi = await NeuzApi.Flush();
        IsBusy  = false;
    }

    /// <summary>
    /// 打开目录
    /// </summary>
    public ICommand OpenDirCommand { get; }

    private async Task OpenDirHandler(object? param)
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

    private async Task OpenConfigHandler(object? param)
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
    public ICommand InstallCommand { get; }

    private async Task InstallHandler(object? param)
    {
        await FlushHandler();

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

        if (param is NeuzApiServiceModel)
        {
            if (NeuzApi.Installed)
            {
                MessageBox.Show($"[{NeuzApi.ServiceName}]已安装");
                IsBusy = false;
                return;
            }

            var wizard = new NeuzApiWizardView()
            {
                DataContext = new NeuzApiWizardViewModel(NeuzApi)
            };
            if (!(wizard.ShowDialog() ?? false))
            {
                IsBusy = false;
                return;
            }

            var packFilePath = wizard.TextBoxPackPath.Text;

            var rs = await NeuzApi.Install(packFilePath, i =>
            {
                IndicatorText = $"{i}%";
            });
            MessageBox.Show(rs ? $"[{NeuzApi.ServiceName}]安装成功" : $"[{NeuzApi.ServiceName}]安装失败");
            IndicatorText = "";
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

        if (param is NeuzApiServiceModel)
        {
            if (NeuzApi.RunningStatus == RunningStatus.Running)
            {
                MessageBox.Show($"[{Redis.ServiceName}]运行中");
                IsBusy = false;
                return;
            }

            var rs = await NeuzApi.UnInstall();
            MessageBox.Show(rs ? $"[{NeuzApi.ServiceName}]卸载成功" : $"[{NeuzApi.ServiceName}]卸载失败");
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
        await FlushHandler();
    }

    public ICommand OpenUrl { get; }

    public async Task OpenUrlHandler(object? param)
    {
        if (param is NeuzApiServiceModel)
        {
            var cli = Cli.Wrap("cmd.exe")
                         .WithArguments(new[] {"/c", "start", $"http://localhost:{NeuzApi.Port}"})
                         .WithValidation(CommandResultValidation.None);
            Log.Information(cli.ToString());
            await cli.ExecuteBufferedAsync();
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
        await FlushHandler();
    }

    #endregion
}