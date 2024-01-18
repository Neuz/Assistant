using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Assistant.Model;
using Assistant.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using MessageBox = System.Windows.MessageBox;

namespace Assistant.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    private readonly FileService  _fileService = Ioc.Default.GetRequiredService<FileService>();

    private readonly GlobalConfig _config = Ioc.Default.GetRequiredService<GlobalConfig>();


    public string Title => "设置";

    // private readonly GlobalConfig _config;

    public string BasePath
    {
        get => _config.BasePath;
        set => SetProperty(_config.BasePath, value, _config, (c, s) => c.BasePath = s);
    }

    public string SourceUrl
    {
        get => _config.SourceUrl;
        set => SetProperty(_config.SourceUrl, value, _config, (c, s) => c.SourceUrl = s);
    }


    [RelayCommand]
    private void Save()
    {
        _fileService.Save(GlobalConfig.ConfigPath, _config);
        MessageBox.Show("保存成功");
        Log.Information("保存配置");
    }

    [RelayCommand]
    private void SelectFolder()
    {
        var dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() == DialogResult.OK) BasePath = dialog.SelectedPath;
    }
}