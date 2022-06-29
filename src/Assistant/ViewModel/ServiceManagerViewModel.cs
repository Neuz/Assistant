using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant.Model.ServiceManager;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

// ReSharper disable InconsistentNaming

namespace Assistant.ViewModel;

public class ServiceManagerViewModel : ObservableObject
{
    public ServiceManagerViewModel()
    {
        FlushCommand = new RelayCommand(FlushCommandHandler);
        _mysql       = MySQLServiceModel.GetDefaultProfile();
    }

    #region 属性

    public string Title => "服务管理";

    #region MySQL

    public string MySQLDisplay => _mysql.DisplayName;

    public bool MySQLInstalled
    {
        get => _mysql.Installed;
        set => SetProperty(_mysql.Installed, value, _mysql, (model, b) => model.Installed = b);
    }

    public RunningStatus MySqlRunningStatus
    {
        get => _mysql.RunningStatus;
        set => SetProperty(_mysql.RunningStatus, value, _mysql, (model, status) => model.RunningStatus = status);
    }

    private readonly MySQLServiceModel _mysql;

    #endregion

    #endregion

    #region 命令

    public ICommand FlushCommand { get; }

    /// <summary>
    /// 刷新
    /// </summary>
    private void FlushCommandHandler()
    {
        MySQLInstalled     = !MySQLInstalled;
        MySqlRunningStatus = RunningStatus.Stopping;
    }

    #endregion
}