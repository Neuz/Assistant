using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Assistant.Messages;
using Assistant.Model.BaseServices;
using Assistant.Model.OperationsTools;
using Assistant.Services;
using Assistant.View.BaseServices;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Assistant.ViewModel;

public partial class DashboardViewModel : ObservableObject
{
    private readonly WinSvcService _winSvcService = Ioc.Default.GetRequiredService<WinSvcService>();
    public string Title => "仪表盘";

    [ObservableProperty]
    private MySQL _mySql = new();
    
    [ObservableProperty]
    private string _mySqlColor = "Gray";

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private void Goto(Type t)
    {
        WeakReferenceMessenger.Default.Send(new GotoMessage<Type>(t));
    }

    [RelayCommand]
    private async Task Flush()
    {
        MySql.WinSvc = _winSvcService.Query(MySql.ServiceName).FirstOrDefault();
        MySqlColor   = GetMyColor(MySql);
    }

    [RelayCommand]
    private async Task Start(object obj)
    {
        if (obj is MySQL)
        {
            await WithBusy(async () =>
            {
                var rs = await _winSvcService.Start(MySql.WinSvc);
            });
        }

        return;
    }

    [RelayCommand]
    private async Task Stop(object obj)
    {
        if (obj is MySQL)
        {
            await WithBusy(async () =>
            {
                var rs = await _winSvcService.Stop(MySql.WinSvc);
            });
        }

        return;
    }


    private async Task WithBusy(Func<Task> func)
    {
        IsBusy = true;

        await func.Invoke();

        IsBusy = false;
    }


    private string GetMyColor(MySQL mySql)
    {
        if (mySql.WinSvc == null) return "Gray"; // 默认色
        return mySql.WinSvc.Status == WinSvc.RunningStatus.Running
                   ? "GreenYellow" // 运行
                   : "Red";        // 停止及其他
    }
}