using Assistant.Model;
using Assistant.Model.Backup;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Assistant.Utils;
using Assistant.View.BackupWizard;
using CommunityToolkit.Mvvm.Messaging;

// ReSharper disable InconsistentNaming

namespace Assistant.ViewModel;

public partial class BackupViewModel : ObservableObject
{
    public string Title => "数据备份";

    private string DbBackupDir => Path.Combine(Global.BackupsDir, "DB_Backup");

    #region Property

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _busyText = "";

    [ObservableProperty]
    private ObservableCollection<BackupFile> _backupFiles = new();

    #endregion


    #region Command

    /// <summary>
    /// 刷新
    /// </summary>
    [RelayCommand]
    private void FlushUI()
    {
        MessageBox.Show("Flush ui");
        // Directory.GetFiles(DbBackupDir,"*.sql")
        //          .Select(f=>new BackupFile())
    }

    /// <summary>
    /// 新建备份
    /// </summary>
    /// <param name="obj"></param>
    [RelayCommand]
    private void AddBackFile(object? obj)
    {
        var wizard = new BackupWizardView();
        wizard.ShowDialog();

    }

    /// <summary>
    /// 还原备份
    /// </summary>
    /// <param name="obj"></param>
    [RelayCommand]
    private void RestoreBackupFile(object? obj)
    {
        MessageBox.Show("Restore");
    }

    [RelayCommand]
    private async Task OpenDir() => await FileUtils.OpenDirectory(DbBackupDir);

    #endregion
}