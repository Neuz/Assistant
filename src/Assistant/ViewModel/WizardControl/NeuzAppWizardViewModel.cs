using Assistant.Model.ServiceManager;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Assistant.ViewModel.WizardControl;

public class NeuzAppWizardViewModel : ObservableObject
{
    public NeuzAppWizardViewModel()
    {
    }


    public NeuzAppWizardViewModel(NeuzAppServiceModel app)
    {
        _app = app;

        PreviewDbSettings = _app.Api.Database.DbSettings.ToString();
        PreviewSql        = _app.Api.Database.CreateDatabaseSql();
    }


    private readonly NeuzAppServiceModel _app = new();

    public string ApiServiceName
    {
        get => _app.Api.ServiceName ?? "N/A";
        set => SetProperty(_app.Api.ServiceName, value, _app, (model, s) => model.Api.ServiceName = s);
    }

    public string ApiServiceDescription
    {
        get => _app.Api.ServiceDescription ?? "N/A";
        set => SetProperty(_app.Api.ServiceDescription, value, _app, (model, s) => model.Api.ServiceDescription = s);
    }

    public int ApiPort
    {
        get => _app.Api.Port;
        set => SetProperty(_app.Api.Port, value, _app, (model, s) => model.Api.Port = s);
    }

    public string DatabaseHost
    {
        get => _app.Api.Database.Host ?? "localhost";
        set
        {
            SetProperty(_app.Api.Database.Host, value, _app, (model, s) => model.Api.Database.Host = s);
            PreviewSql        = _app.Api.Database.CreateDatabaseSql();
            PreviewDbSettings = _app.Api.Database.DbSettings.ToString();
        }
    }

    public int DatabasePort
    {
        get => _app.Api.Database.Port;
        set
        {
            SetProperty(_app.Api.Database.Port, value, _app, (model, s) => model.Api.Database.Port = s);
            PreviewSql        = _app.Api.Database.CreateDatabaseSql();
            PreviewDbSettings = _app.Api.Database.DbSettings.ToString();
        }
    }

    public string DatabaseUser
    {
        get => _app.Api.Database.User ?? "root";
        set
        {
            SetProperty(_app.Api.Database.User, value, _app, (model, s) => model.Api.Database.User = s);
            PreviewSql        = _app.Api.Database.CreateDatabaseSql();
            PreviewDbSettings = _app.Api.Database.DbSettings.ToString();
        }
    }

    public string DatabasePassword
    {
        get => _app.Api.Database.Password ?? "";
        set
        {
            SetProperty(_app.Api.Database.Password, value, _app, (model, s) => model.Api.Database.Password = s);
            PreviewSql        = _app.Api.Database.CreateDatabaseSql();
            PreviewDbSettings = _app.Api.Database.DbSettings.ToString();
        }
    }

    public string DatabaseDbPrefix
    {
        get => _app.Api.Database.DbPrefix ?? "";
        set
        {
            SetProperty(_app.Api.Database.DbPrefix, value, _app, (model, s) => model.Api.Database.DbPrefix = s);
            PreviewSql        = _app.Api.Database.CreateDatabaseSql();
            PreviewDbSettings = _app.Api.Database.DbSettings.ToString();
        }
    }

    public string PreviewSql
    {
        get => _previewSql;
        set => SetProperty(ref _previewSql, value);
    }


    private string _previewSql = "";

    public string PreviewDbSettings
    {
        get => _previewDbSettings;
        set => SetProperty(ref _previewDbSettings, value);
    }

    private string _previewDbSettings = "";
}