using Assistant.Model.ServiceManager;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Assistant.ViewModel.WizardControl;

public class NeuzAdapterK3WiseWizardViewModel : ObservableObject
{
    public NeuzAdapterK3WiseWizardViewModel()
    {
    }


    public NeuzAdapterK3WiseWizardViewModel(K3WiseAdapterService wise)
    {
        _wise       = wise;
        ConfigText = _wise.ToConfigString();
    }


    public string ServiceName
    {
        get => _wise.ServiceName ?? string.Empty;
        set => SetProperty(_wise.ServiceName, value, _wise, (model, s) => model.ServiceName = s);
    }

    public string ServiceDescription
    {
        get => _wise.ServiceDescription ?? string.Empty;
        set => SetProperty(_wise.ServiceDescription, value, _wise, (model, s) => model.ServiceDescription = s);
    }

    public int Port
    {
        get => _wise.Port;
        set
        {
            SetProperty(_wise.Port, value, _wise, (model, i) => model.Port = i);
            ConfigText = _wise.ToConfigString();
        }
    }
    

    public string ConfigText
    {
        get => _configText;
        set => SetProperty(ref _configText, value);
    }

    private string _configText = "";

    private readonly K3WiseAdapterService _wise = new();
}