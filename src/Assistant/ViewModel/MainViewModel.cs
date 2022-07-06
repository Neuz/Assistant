using System;
using System.Windows;
using Assistant.View;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Syncfusion.UI.Xaml.NavigationDrawer;
using System.Windows.Controls;
using System.Windows.Input;
using Serilog;

namespace Assistant.ViewModel;

public class MainViewModel : ObservableObject
{
    private UserControl? _currentView;
    public string Title => "Neuz 助手";
    
    private readonly LogView _logView;

    public UserControl? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public static readonly object LogSyncLock = new();

    public MainViewModel()
    {
        ClickCommand = new RelayCommand<object?>(ClickHandler);
        _logView     = new LogView();
        // 日志配置
        const string outputTemplate = "[{Level:u3}] [{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.RichTextBox(_logView.LogTextBox, outputTemplate: outputTemplate, syncRoot: LogSyncLock)
                    .CreateLogger();
    }


    public ICommand ClickCommand { get; }

    private void ClickHandler(object? obj)
    {
        var t = obj as Type;

        if (t == typeof(ServiceManagerView)) CurrentView = new ServiceManagerView();

        if (t == typeof(SystemInfoView)) CurrentView = new SystemInfoView();

        if (t == typeof(ToolsView)) CurrentView = new ToolsView();

        if (t == typeof(LogView)) CurrentView = _logView;
    }
}