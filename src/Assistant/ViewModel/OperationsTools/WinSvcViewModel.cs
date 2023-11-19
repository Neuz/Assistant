using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Assistant.Model.OperationsTools;
using Assistant.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.UI.Xaml.Grid;

namespace Assistant.ViewModel.OperationsTools;

public partial class WinSvcViewModel : ObservableObject
{
    public string Title => "Windows 服务管理";


    [ObservableProperty]
    private List<WinSvc> _winSvcList = new();

    [RelayCommand]
    private async Task Flush(object obj)
    {
        var s = Ioc.Default.GetRequiredService<WinSvcService>();
        WinSvcList = s.Query();
    }
}