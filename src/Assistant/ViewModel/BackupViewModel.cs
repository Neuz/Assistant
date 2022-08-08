using CommunityToolkit.Mvvm.ComponentModel;

namespace Assistant.ViewModel;

public class BackupViewModel : ObservableObject
{
    public string Title => "数据备份";

    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }


    private string _busyText = "";

    public string BusyText
    {
        get => _busyText;
        set => SetProperty(ref _busyText, value);
    }

    private string _databaseHost = "localhost";

    public string DatabaseHost
    {
        get => _databaseHost;
        set => SetProperty(ref _databaseHost, value);
    }

    private int _databasePort = 10001;

    public int DatabasePort
    {
        get => _databasePort;
        set => SetProperty(ref _databasePort, value);
    }


    private string _databaseUser = "root";

    public string DatabaseUser
    {
        get => _databaseUser;
        set => SetProperty(ref _databaseUser, value);
    }

    private string _databasePassword = "Neuz123";

    public string DatabasePassword
    {
        get => _databasePassword;
        set => SetProperty(ref _databasePassword, value);
    }
}