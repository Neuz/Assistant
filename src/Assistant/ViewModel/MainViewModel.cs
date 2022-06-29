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
        const string outputTemplate = "[{Level:u3}] [{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.RichTextBox(_logView.RichTextBox, outputTemplate: outputTemplate, syncRoot: LogSyncLock)
                    .CreateLogger();
    }

    private void ClickHandler(object? obj)
    {
        var aa = obj as NavigationItem;
        switch (aa?.Header.ToString())
        {
            case "系统环境检测":
                _systemInfoView ??= new SystemInfoView();
                CurrentView     =   _systemInfoView;
                break;
            case "服务管理":
                _serviceManagerView ??= new ServiceManagerView();
                CurrentView         =   _serviceManagerView;
                break;
            case "日志":
                CurrentView =   _logView;
                break;
            default:
                MessageBox.Show("error");
                break;
        }
    }


    public ICommand ClickCommand { get; }
}