using Assistant.Model.ServiceManager;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Assistant.ViewModel.WizardControl;

public class NeuzAdapterKisWizardViewModel : ObservableObject
{
    public NeuzAdapterKisWizardViewModel()
    {
    }


    public NeuzAdapterKisWizardViewModel(KisAdapterService kis)
    {
        _kis       = kis;
        ConfigText = _kis.ToConfigString();
    }


    public string ServiceName
    {
        get => _kis.ServiceName ?? string.Empty;
        set => SetProperty(_kis.ServiceName, value, _kis, (model, s) => model.ServiceName = s);
    }

    public string ServiceDescription
    {
        get => _kis.ServiceDescription ?? string.Empty;
        set => SetProperty(_kis.ServiceDescription, value, _kis, (model, s) => model.ServiceDescription = s);
    }

    public int Port
    {
        get => _kis.Port;
        set
        {
            SetProperty(_kis.Port, value, _kis, (model, i) => model.Port = i);
            ConfigText = _kis.ToConfigString();
        }
    }

    public string AccountNumber
    {
        get => _kis.KingdeeAccount.Number;
        set
        {
            SetProperty(_kis.KingdeeAccount.Number, value, _kis, (model, s) => model.KingdeeAccount.Number = s);
            ConfigText = _kis.ToConfigString();
        }
    }

    public string AccountUser
    {
        get => _kis.KingdeeAccount.User;
        set
        {
            SetProperty(_kis.KingdeeAccount.User, value, _kis, (model, s) => model.KingdeeAccount.User = s);
            ConfigText = _kis.ToConfigString();
        }
    }

    public string AccountPwd
    {
        get => _kis.KingdeeAccount.Password;
        set
        {
            SetProperty(_kis.KingdeeAccount.Password, value, _kis, (model, s) => model.KingdeeAccount.Password = s);
            ConfigText = _kis.ToConfigString();
        }
    }

    public string ConfigText
    {
        get => _configText;
        set => SetProperty(ref _configText, value);
    }

    private string _configText = "";

    private readonly KisAdapterService _kis = new();
}