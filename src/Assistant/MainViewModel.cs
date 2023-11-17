using Assistant.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using Assistant.View.BaseServices;
using Serilog;

namespace Assistant;

public class MainViewModel : ObservableObject
{
    private UserControl? _currentView;
    public string Title => "Neuz 助手";


    public UserControl? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public MainViewModel()
    {
        ClickCommand = new RelayCommand<object?>(ClickHandler);
        // 日志配置
        const string outputTemplate = "[{Level:u3}] [{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.File("Assistant.log", shared: true, rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate)
                    .CreateLogger();

        // ClickHandler(typeof(SystemInfoView));
        // CurrentView = new SystemInfoView();
    }


    public ICommand ClickCommand { get; }

    private void ClickHandler(object? obj)
    {
        var t= obj as Type;

        if (t == typeof(MySQLView)) CurrentView = new MySQLView();
    }
}