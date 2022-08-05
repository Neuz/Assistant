﻿using Assistant.Model.ServiceManager;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Assistant.ViewModel.WizardControl;

public class MySQLWizardViewModel : ObservableObject
{
    public MySQLWizardViewModel()
    {
    }


    public MySQLWizardViewModel(MySqlService mysql)
    {
        _model = mysql;
    }


    public string ServiceName
    {
        get => _model.ServiceName ?? string.Empty;
        set => SetProperty(_model.ServiceName, value, _model, (model, s) => model.ServiceName = s);
    }

    public string ServiceDescription
    {
        get => _model.ServiceDescription ?? string.Empty;
        set => SetProperty(_model.ServiceDescription, value, _model, (model, s) => model.ServiceDescription = s);
    }

    public int Port
    {
        get => _model.MySQLConfig.Port;
        set => SetProperty(_model.MySQLConfig.Port, value, _model, (model, i) => model.MySQLConfig.Port = i);
    }

    public string Password
    {
        get => _model.MySQLConfig.Password;
        set => SetProperty(_model.MySQLConfig.Password, value, _model, (model, s) => model.MySQLConfig.Password = s);
    }


    private readonly MySqlService _model = new();
}