using Assistant.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Controls;

namespace Assistant;

public partial class MainViewModel : ObservableRecipient
{
    public string Title => "Neuz 助手";

    public MainViewModel()
    {
        IsActive    =   true;   
        CurrentView ??= Ioc.Default.GetService<DashboardView>();
    }
    

    [ObservableProperty]
    private ContentControl? _currentView;


    [RelayCommand]
    private void Click(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        CurrentView = (ContentControl)Ioc.Default.GetService(type)!;
    }


    
}