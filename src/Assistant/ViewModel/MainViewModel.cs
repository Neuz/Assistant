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

    private ServiceManagerView? _serviceManagerView;
    private SystemInfoView?     _systemInfoView;
    private ToolsView?          _toolsView;
    private LogView?            _logView;

    public UserControl? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public static readonly object LogSyncLock = new();

    public MainViewModel()
    {
        ClickCommand = new RelayCommand<object?>(ClickHandler);

        // 日志配置
        _logView = new LogView();
        const string outputTemplate = "[{Level:u3}] [{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.RichTextBox(_logView.LogTextBox, outputTemplate: outputTemplate, syncRoot: LogSyncLock)
                    .CreateLogger();
    }

    private void ClickHandler(object? obj)
    {

        var t = obj as Type;

        if (t == typeof(ServiceManagerView))
        {
            _serviceManagerView ??= new ServiceManagerView();
            CurrentView         =   _serviceManagerView;
        }

        if (t == typeof(SystemInfoView))
        {
            _systemInfoView ??= new SystemInfoView();
            CurrentView     =   _systemInfoView;
        }

        if (t == typeof(ToolsView))
        {
            _toolsView  ??= new ToolsView();
            CurrentView =   _toolsView;
        }

        if (t == typeof(LogView)) CurrentView = _logView;
    }


    public ICommand ClickCommand { get; }
}