using Assistant.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using Assistant.Messages;
using Assistant.View.BaseServices;
using Assistant.ViewModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Serilog;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;

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
    private UserControl? _currentView;
    

    [RelayCommand]
    private void Click(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        CurrentView = (UserControl)Ioc.Default.GetService(type)!;
    }


    protected override void OnActivated()
    {
        Messenger.Register<MainViewModel,GotoMessage<Type>>(this, (r, m) =>
        {
            CurrentView = (UserControl)Ioc.Default.GetService(m.Value)!;
        });
    }
    
}