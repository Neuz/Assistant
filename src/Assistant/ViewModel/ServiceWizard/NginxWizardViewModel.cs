using Assistant.Model.ServiceManager;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Assistant.ViewModel.ServiceWizard;

public class NginxWizardViewModel : ObservableObject
{
    public NginxWizardViewModel()
    {
    }


    public NginxWizardViewModel(NginxService nginx)
    {
        _nginx      = nginx;
        _configText = _nginx.NginxConfig.ToString();
    }


    public string ServiceName
    {
        get => _nginx.ServiceName ?? string.Empty;
        set => SetProperty(_nginx.ServiceName, value, _nginx, (model, s) => model.ServiceName = s);
    }

    public string ServiceDescription
    {
        get => _nginx.ServiceDescription ?? string.Empty;
        set => SetProperty(_nginx.ServiceDescription, value, _nginx, (model, s) => model.ServiceDescription = s);
    }

    public int WebPort
    {
        get => _nginx.NginxConfig.WebPort;
        set
        {
            SetProperty(_nginx.NginxConfig.WebPort, value, _nginx, (model, i) => model.NginxConfig.WebPort = i);
            ConfigText = _nginx.NginxConfig.ToString();
        }
    }

    public int PdaPort
    {
        get => _nginx.NginxConfig.PdaPort;
        set
        {
            SetProperty(_nginx.NginxConfig.PdaPort, value, _nginx, (model, i) => model.NginxConfig.PdaPort = i);
            ConfigText = _nginx.NginxConfig.ToString();
        }
    }

    public string ConfigText
    {
        get => _configText;
        set => SetProperty(ref _configText, value);
    }

    private string _configText = "";

    private readonly NginxService _nginx = new();
}