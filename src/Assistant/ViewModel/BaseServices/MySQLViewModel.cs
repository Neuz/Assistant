using Assistant.Model;
using Assistant.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using Log = Serilog.Log;

// ReSharper disable InconsistentNaming

namespace Assistant.ViewModel.BaseServices;

public partial class MySQLViewModel : ObservableObject
{
    private readonly GlobalConfig  _global        = Ioc.Default.GetRequiredService<GlobalConfig>();
    private readonly WinSvcService _winSvcService = Ioc.Default.GetRequiredService<WinSvcService>();
    private readonly FileService   _fileService   = Ioc.Default.GetRequiredService<FileService>();
    private readonly StatService _statService = Ioc.Default.GetRequiredService<StatService>();

    public string Title => "MySQL";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _busyText;

    [ObservableProperty]
    private MySQLDefault _mysqlDefault = new();

    [ObservableProperty]
    private WinSvc? _winService;

    [ObservableProperty]
    private bool _isShowInfoPanel;

    [ObservableProperty]
    private bool _isShowInstallPanel;

    [ObservableProperty]
    private bool _isEnabledStart;

    [ObservableProperty]
    private bool _isEnabledStop;

    [ObservableProperty]
    private bool _isEnabledRestart;

    [ObservableProperty]
    private bool _isEnabledCreate;

    [ObservableProperty]
    private bool _isEnabledDelete;

    [ObservableProperty]
    private string? _zipFile;


    [ObservableProperty]
    private IList<Stats> _currStatus;

    private async Task WithBusy(Func<Task> action)
    {
        IsBusy = true;

        try
        {
            await action.Invoke();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            MessageBox.Show(e.Message, "ERR", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        IsBusy = false;
    }

    /// <summary>
    /// 启动服务
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task SvcStart()
    {
        await WithBusy(async () =>
        {
            BusyText = "正在启动服务...";
            await _winSvcService.Start(MysqlDefault.ServiceName);
        });
        await Flush();
    }

    /// <summary>
    /// 停止服务
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task SvcStop()
    {
        await WithBusy(async () =>
        {
            BusyText = "正在停止服务...";
            await _winSvcService.Stop(MysqlDefault.ServiceName);
        });
        await Flush();
    }

    /// <summary>
    /// 重启服务
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task SvcRestart()
    {
        await WithBusy(async () =>
        {
            BusyText = "正在停止服务...";
            await _winSvcService.Stop(MysqlDefault.ServiceName);
            await Task.Delay(1000);
            BusyText = "正在启动服务...";
            await _winSvcService.Start(MysqlDefault.ServiceName);
        });
        await Flush();
    }


    /// <summary>
    /// 创建服务
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task SvcCreate()
    {
        await WithBusy(async () =>
        {
            BusyText = $"正在创建服务[{MysqlDefault.ServiceName}]...";

            var binPath = MysqlDefault.GetWinSvcBinPath(_global.BasePath);

            await _winSvcService.Create(MysqlDefault.ServiceName, binPath, MysqlDefault.DisplayName, MysqlDefault.Description);
        });
        await Flush();
    }

    /// <summary>
    /// 卸载服务
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task SvcDelete()
    {
        await WithBusy(async () =>
        {
            BusyText = "正在卸载服务...";
            await _winSvcService.Delete(MysqlDefault.ServiceName);
        });
        await Flush();
    }

    /// <summary>
    /// 刷新
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task Flush()
    {
        await WithBusy(async () =>
        {
            BusyText = "刷新中";
            await Task.Run(() =>
            {
                var destDir = Path.Combine(_global.BasePath, MysqlDefault.BaseDir);
                // 用目录下是否存在文件 判断 是否安装
                var isInstall = Directory.Exists(destDir) && Directory.GetFiles(destDir).Any();
                IsShowInfoPanel    = isInstall;
                IsShowInstallPanel = !isInstall;

                WinService = _winSvcService.Query(MysqlDefault.ServiceName);

                //设置上下文菜单启用
                IsEnabledCreate = WinService == null;
                IsEnabledDelete = WinService != null;

                var status = WinService?.ServiceController?.Status;

                if (status == null)
                {
                    IsEnabledStart = IsEnabledStop = IsEnabledRestart = false;
                }
                else
                {
                    IsEnabledStart   = status == ServiceControllerStatus.Stopped;
                    IsEnabledRestart = IsEnabledStop = WinService?.ServiceController?.CanStop ?? false;
                }
            });

            if (WinService?.Status == ServiceControllerStatus.Running)
            {
                // 加载 Redis 运行监测
                CurrStatus = ( await _statService.GetMySQLStatsList(MysqlDefault.User,MysqlDefault.Password)).OrderBy(stats => stats.Order).ToList();
            }

            // 为了界面丝滑，delay一下
            await Task.Delay(1000);
        });
    }


    /// <summary>
    /// 选择zip
    /// </summary>
    [RelayCommand]
    private void SelectZip()
    {
        var ofd = new OpenFileDialog {Filter = "压缩文件(*.zip)|*.zip|所有文件(*.*)|*.*"};

        if (ofd.ShowDialog() ?? false) ZipFile = ofd.FileName;
    }


    /// <summary>
    /// 本地安装
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task LocalInstall()
    {
        await WithBusy(async () =>
        {
            if (string.IsNullOrEmpty(_global.BasePath)) ThrowHelper.ThrowArgumentException("基础目录未设置，请在[设置]中设置基础目录");

            if (string.IsNullOrEmpty(ZipFile)) ThrowHelper.ThrowArgumentException("压缩文件为空");

            var destDir = Path.Combine(_global.BasePath, MysqlDefault.BaseDir);
            var msg     = $"将安装至以下目录\r\n{destDir}\r\n是否继续?";
            if (MessageBox.Show(msg, "安装确认", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            BusyText = "正在解压...";
            Log.Information($"{ZipFile} 解压至 {destDir}");
            await _fileService.ZipExtract(ZipFile, destDir);

            if (MessageBox.Show("是否创建Windows服务并启动？", "注册Windows服务确认", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            BusyText = "创建服务...";
            Log.Information($"创建服务[{MysqlDefault.ServiceName}]");

            var binPath = MysqlDefault.GetWinSvcBinPath(_global.BasePath);
            await _winSvcService.Create(MysqlDefault.ServiceName, binPath, MysqlDefault.DisplayName, MysqlDefault.Description);

            // 初始化my.ini
            var baseDir = Path.Combine(_global.BasePath, MysqlDefault.BaseDir);
            _fileService.InitMySqlIni(baseDir);

            BusyText = "服务启动...";
            Log.Information($"服务启动[{MysqlDefault.ServiceName}]");
            await _winSvcService.Start(MysqlDefault.ServiceName);
        });

        await Flush();
    }
}