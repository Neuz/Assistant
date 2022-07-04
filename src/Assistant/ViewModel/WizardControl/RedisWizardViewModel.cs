using Assistant.Model.ServiceManager;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Assistant.ViewModel.WizardControl;

public class RedisWizardViewModel : ObservableObject
{
    public RedisWizardViewModel()
    {
    }


    public RedisWizardViewModel(RedisServiceModel redis)
    {
        _redis      = redis;
        _configText = _redis.GetConfigText() ?? string.Empty;
    }


    public string ServiceName
    {
        get => _redis.ServiceName ?? string.Empty;
        set => SetProperty(_redis.ServiceName, value, _redis, (model, s) => model.ServiceName = s);
    }

    public string ServiceDescription
    {
        get => _redis.ServiceDescription ?? string.Empty;
        set => SetProperty(_redis.ServiceDescription, value, _redis, (model, s) => model.ServiceDescription = s);
    }

    public int Port
    {
        get => _redis.RedisConfig.Port;
        set
        {
            SetProperty(_redis.RedisConfig.Port, value, _redis, (model, i) => model.RedisConfig.Port = i);
            ConfigText = _redis.GetConfigText()!;
        }
    }

    public string ConfigText
    {
        get => _configText;
        set => SetProperty(ref _configText, value);
    }

    private string _configText = "";

    private readonly RedisServiceModel _redis = new();
}