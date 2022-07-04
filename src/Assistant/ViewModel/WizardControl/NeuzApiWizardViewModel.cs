using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Assistant.Model.ServiceManager;
using Assistant.Utils;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;
// ReSharper disable InconsistentNaming

namespace Assistant.ViewModel.WizardControl;

public class NeuzApiWizardViewModel : ObservableObject
{
    public NeuzApiWizardViewModel()
    {
    }


    public NeuzApiWizardViewModel(NeuzApiServiceModel neuzApi)
    {
        _neuzApi    = neuzApi;
        _configText = _neuzApi.GetDbSettingsSerializer();
        _previewSQL = _neuzApi.GetCreateDbSQL();
    }

    public string ServiceName
    {
        get => _neuzApi.ServiceName ?? string.Empty;
        set => SetProperty(_neuzApi.ServiceName, value, _neuzApi, (model, s) => model.ServiceName = s);
    }

    public string ServiceDescription
    {
        get => _neuzApi.ServiceDescription ?? string.Empty;
        set => SetProperty(_neuzApi.ServiceDescription, value, _neuzApi, (model, s) => model.ServiceDescription = s);
    }

    public int Port
    {
        get => _neuzApi.Port;
        set => SetProperty(_neuzApi.Port, value, _neuzApi, (model, i) => model.Port = i);
    }

    public string MySQLHost
    {
        get => _neuzApi.MySQLHost;
        set
        {
            SetProperty(_neuzApi.MySQLHost, value, _neuzApi, (model, s) => model.MySQLHost = s);
            ConfigText = _neuzApi.GetDbSettingsSerializer();
        }
    }


    public int MySQLPort
    {
        get => _neuzApi.MySQLPort;
        set
        {
            SetProperty(_neuzApi.MySQLPort, value, _neuzApi, (model, i) => model.MySQLPort = i);
            ConfigText = _neuzApi.GetDbSettingsSerializer();
        }
    }

    public string MySQLUser
    {
        get => _neuzApi.MySQLUser;
        set
        {
            SetProperty(_neuzApi.MySQLUser, value, _neuzApi, (model, s) => model.MySQLUser = s);
            ConfigText = _neuzApi.GetDbSettingsSerializer();
        }
    }

    public string MySQLPassword
    {
        get => _neuzApi.MySQLPassword;
        set
        {
            SetProperty(_neuzApi.MySQLPassword, value, _neuzApi, (model, s) => model.MySQLPassword = s);
            ConfigText = _neuzApi.GetDbSettingsSerializer();
        }
    }

    public string ConfigText
    {
        get => _configText;
        set => SetProperty(ref _configText, value);
    }

    private string _configText = "";

    public string Prefix
    {
        get => _neuzApi.DbPrefix;
        set
        {
            SetProperty(_neuzApi.DbPrefix,value,_neuzApi,(model, s) => model.DbPrefix = s);
            PreviewSQL = _neuzApi.GetCreateDbSQL();
            ConfigText = _neuzApi.GetDbSettingsSerializer();
        }
    }


    public string PreviewSQL
    {
        get => _previewSQL;
        set => SetProperty(ref _previewSQL, value);
    }

    private string _previewSQL = "";
    


    private readonly NeuzApiServiceModel _neuzApi = new();
}