using System.Windows;
using Assistant.View;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Syncfusion.UI.Xaml.NavigationDrawer;
using System.Windows.Controls;
using System.Windows.Input;

namespace Assistant.ViewModel;

public class MainViewModel : ObservableObject
{
    private UserControl _contentView;
    public string Title => "Neuz 助手";

    public UserControl ContentView
    {
        get => _contentView;
        set => SetProperty(ref _contentView, value);
    }

    public MainViewModel()
    {
        ClickCommand = new RelayCommand<object?>(ClickHandler);
        _contentView = new SystemInfoView();
    }

    private void ClickHandler(object? obj)
    {
        var aa = obj as NavigationItem;
        switch (aa?.Header.ToString())
        {
            case "系统环境检测":
                ContentView = new SystemInfoView();
                break;
            case "服务管理":
                ContentView = new ServiceManagerView();
                break;
            default:
                MessageBox.Show("error");
                break;
        }
    }


    public ICommand ClickCommand { get; }
}