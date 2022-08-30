using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Assistant.Messages;
using Assistant.Model.Backup;
using Assistant.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MySqlConnector;
using Syncfusion.Data.Extensions;

namespace Assistant.ViewModel.BackupWizard;

public partial class BackupWizardViewModel : ObservableObject, IRecipient<LoadDatabaseMessage>, IRecipient<LoadTablesMessage>
{
    public BackupWizardViewModel()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);
        SelectedTables.CollectionChanged += SelectedTablesOnCollectionChanged;
    }


    public BackupConfig BackupConfig { get; set; } = new();

    #region Property

    public string Host
    {
        get => BackupConfig.Host;
        set => SetProperty(BackupConfig.Host, value, BackupConfig, (config, s) => config.Host = s);
    }

    public int Port
    {
        get => BackupConfig.Port;
        set => SetProperty(BackupConfig.Port, value, BackupConfig, (config, i) => config.Port = i);
    }

    public string User
    {
        get => BackupConfig.UserID;
        set => SetProperty(BackupConfig.UserID, value, BackupConfig, (config, s) => config.UserID = s);
    }

    public string Password
    {
        get => BackupConfig.Password;
        set => SetProperty(BackupConfig.Password, value, BackupConfig, (config, s) => config.Password = s);
    }

    public string Note
    {
        get => BackupConfig.Note;
        set => SetProperty(BackupConfig.Note, value, BackupConfig, (config, s) => config.Note = s);
    }

    public string SelectedDatabase
    {
        get => BackupConfig.Database;
        set => SetProperty(BackupConfig.Database, value, BackupConfig, (config, s) => config.Database = s);
    }

    [ObservableProperty]
    private ObservableCollection<object> _selectedTables = new();

    private void SelectedTablesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        BackupConfig.Tables = SelectedTables.Select(s => s.ToString() ?? "").ToList();
    }

    [ObservableProperty]
    private ObservableCollection<string> _databases = new();

    [ObservableProperty]
    private ObservableCollection<string> _tables = new();

    public bool ExportProcedures
    {
        get => BackupConfig.ExportProcedures;
        set => SetProperty(BackupConfig.ExportProcedures, value, BackupConfig, (config, b) => config.ExportProcedures = b);
    }

    public bool ExportFunctions
    {
        get => BackupConfig.ExportFunctions;
        set => SetProperty(BackupConfig.ExportFunctions, value, BackupConfig, (config, b) => config.ExportFunctions = b);
    }

    public bool ExportTriggers
    {
        get => BackupConfig.ExportTriggers;
        set => SetProperty(BackupConfig.ExportTriggers, value, BackupConfig, (config, b) => config.ExportTriggers = b);
    }


    public bool ExportViews
    {
        get => BackupConfig.ExportViews;
        set => SetProperty(BackupConfig.ExportViews, value, BackupConfig, (config, b) => config.ExportViews = b);
    }

    public bool ExportEvents
    {
        get => BackupConfig.ExportEvents;
        set => SetProperty(BackupConfig.ExportEvents, value, BackupConfig, (config, b) => config.ExportEvents = b);
    }

    #endregion


    [RelayCommand]
    private async Task BackupRun()
    {
    }


    public async void Receive(LoadDatabaseMessage message)
    {
        _databases.Clear();
        try
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server            = Host,
                Port              = Convert.ToUInt32(Port),
                UserID            = User,
                Password          = Password,
                ConnectionTimeout = 10
            };

            (await MySQLUtils.GetAllDatabases(builder)).ForEach(d => Databases.Add(d));
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    public async void Receive(LoadTablesMessage message)
    {
        Tables.Clear();
        var builder = new MySqlConnectionStringBuilder
        {
            Server            = Host,
            Port              = Convert.ToUInt32(Port),
            UserID            = User,
            Password          = Password,
            ConnectionTimeout = 10
        };

        (await MySQLUtils.GetAllTables(builder, SelectedDatabase)).ForEach(d => Tables.Add(d));
    }
}