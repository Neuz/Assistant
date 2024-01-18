using Assistant.Model;
using CliWrap;
using CliWrap.Buffered;
using CommunityToolkit.Diagnostics;
using Syncfusion.Data.Extensions;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using CliWrap.Exceptions;
using Serilog;

// ReSharper disable ReplaceWithSingleCallToAny

namespace Assistant.Services;

public class WinSvcService
{
    public WinSvc? Query(string serviceName)
    {
        Guard.IsNotNullOrEmpty(serviceName);
        return ServiceController.GetServices()
                                .Where(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                                .Select(s => new WinSvc
                                 {
                                     ServiceController = s
                                 })
                                .FirstOrDefault();
    }

    public async Task<BufferedCommandResult> Create(string serviceName, string binPath, string displayName = "", string description = "")
    {
        Guard.IsNotNullOrEmpty(serviceName);
        Guard.IsNotNullOrEmpty(binPath);

        var args = string.IsNullOrEmpty(displayName)
                       ? new[] {"create", serviceName, "binpath=", $"{binPath}", "start= auto"}
                       : new[] {"create", serviceName, "binpath=", $"{binPath}", "displayname=", $"{displayName}", "start=", "auto"};

        var cli = Cli.Wrap("sc.exe").WithArguments(args);
        Log.Information($"CLI: {cli}");
        await cli.ExecuteBufferedAsync();

        cli = Cli.Wrap("sc.exe").WithArguments(new[] {"description", serviceName, $"{description}"});
        Log.Information($"CLI: {cli}");
        return await cli.ExecuteBufferedAsync();
    }


    public async Task<BufferedCommandResult> Delete(string serviceName)
    {
        Guard.IsNotNullOrEmpty(serviceName);

        var cli = Cli.Wrap("sc.exe").WithArguments(new[] {"delete", serviceName});
        Log.Information($"CLI: {cli}");
        return await cli.ExecuteBufferedAsync();
    }


    /// <summary>
    /// 启动服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public async Task<BufferedCommandResult> Start(string serviceName)
    {
        var cli = Cli.Wrap("net")
                     .WithArguments(new[] {"start", serviceName});
        return await cli.ExecuteBufferedAsync();
    }


    /// <summary>
    /// 停止服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public async Task<BufferedCommandResult> Stop(string serviceName)
    {
        var cli = Cli.Wrap("net")
                     .WithArguments(new[] {"stop", serviceName});
        return await cli.ExecuteBufferedAsync();
    }
}