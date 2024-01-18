using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;
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

    public DashboardViewModel()
    {
        Flush().GetAwaiter().GetResult();
    }
    
    public string Title => "仪表盘";

    
    [ObservableProperty]
    private string _mySqlColor = "Gray";

    [ObservableProperty]
    private bool _isBusy;


    [RelayCommand]
    private async Task Flush()
    {
        
    }

    [RelayCommand]
    private async Task Start(object obj)
    {
       

        return;
    }

    [RelayCommand]
    private async Task Stop(object obj)
    {

        return;
    }


    private async Task WithBusy(Func<Task> func)
    {
        IsBusy = true;

        await func.Invoke();

        IsBusy = false;
    }

}