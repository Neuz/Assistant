using Assistant.Model;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Assistant.Utils;

namespace Assistant.ViewModel;

public class ToolsViewModel : ObservableObject
{
    public ToolsViewModel()
    {
        RunCommand  = new AsyncRelayCommand<object>(RunHandler);
        HelpCommand = new AsyncRelayCommand<object>(HelpHandler);
    }

    private async Task HelpHandler(object? arg)
    {
        if (arg is ToolModel model) await Task.Run(() => model.HelpAction?.Invoke(model));
    }

    private async Task RunHandler(object? arg)
    {
        if (arg is ToolModel model) await Task.Run(() => model.RunAction?.Invoke(model));
    }

    public ICommand RunCommand { get; set; }
    public ICommand HelpCommand { get; set; }

    public ToolModel[] Tools { get; } =
    {
        new()
        {
            Name        = "Beekeeper Studio",
            Icon        = Application.Current.FindResource("Svg.BeekeeperStudio") as Geometry,
            IconColor   = "#FAD83B",
            Description = "Open Source SQL Editor and Database Manager A modern, easy to use, and good looking SQL client for MySQL, Postgres, SQLite, SQL Server, and more.",
            BinPath     = Path.Combine(Global.CurrentDir, "Tools", "BeekeeperStudio", "BeekeeperStudio.exe"),
            RunAction =  model =>
            {
                if (!string.IsNullOrEmpty(model.BinPath)) Cli.Wrap(model.BinPath).ExecuteAsync();
            },
            HelpAction = obj => { _ = FileUtils.OpenUrl("https://www.beekeeperstudio.io/"); }
        },
        new()
        {
            Name        = "远程桌面连接",
            Icon        = Application.Current.FindResource("Svg.RemoteDesktop") as Geometry,
            IconColor   = "#FAD83B",
            Description = "微软远程桌面控制",
            BinPath     = "mstsc.exe",
            RunAction = model =>
            {
                if (!string.IsNullOrEmpty(model.BinPath)) Cli.Wrap(model.BinPath).ExecuteAsync();
            },
            HelpAction = obj => { _ = FileUtils.OpenUrl("https://go.microsoft.com/fwlink/?LinkId=528886"); }
        }
    };
}