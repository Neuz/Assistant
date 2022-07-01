using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant.Model;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace Assistant.ViewModel
{
    public class ToolsViewModel: ObservableObject
    {
        public ToolsViewModel()
        {
            RunCommand  = new AsyncRelayCommand<object>(RunHandler);
            HelpCommand = new AsyncRelayCommand<object>(HelpHandler);
        }

        private async Task HelpHandler(object? arg)
        {
            await Cli.Wrap("cmd.exe")
                     .WithArguments(new[]{ "/c", "start","https://www.beekeeperstudio.io/" })
                     .WithValidation(CommandResultValidation.None)
                     .ExecuteBufferedAsync();
        }

        private async Task RunHandler(object? param)
        {
            if (param?.ToString() == "BeekeeperStudio")
            {
                var path = Path.Combine(Global.CurrentDir, "Tools", "BeekeeperStudio", "BeekeeperStudio.exe");
                await Cli.Wrap(path).ExecuteAsync();

            }
            
        }

        public ICommand RunCommand { get; set;}
        public ICommand HelpCommand { get; set;}
    }
}
