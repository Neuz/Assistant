using Assistant.Model.ServiceManager;
using CliWrap;
using CliWrap.Buffered;
using System;
using System.Threading.Tasks;

namespace Assistant.Utils;

public class WinServiceUtils
{
    public static async Task<bool> StopService(string serviceName)
    {
        var cli = Cli.Wrap("net")
                     .WithArguments(new[] { "stop", serviceName })
                     .WithValidation(CommandResultValidation.ZeroExitCode);
        var rs = await cli.ExecuteBufferedAsync();
        return rs.ExitCode == 0;
    }

    public static async Task<bool> StartService(string serviceName)
    {
        var cli = Cli.Wrap("net")
                     .WithArguments(new[] { "start", serviceName })
                     .WithValidation(CommandResultValidation.ZeroExitCode);
        var rs = await cli.ExecuteBufferedAsync();
        return rs.ExitCode == 0;
    }

    /// <summary>
    /// 删除 Windows 服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public static async Task<bool> DeleteService(string serviceName)
    {
        var cli = Cli.Wrap("sc.exe")
                     .WithArguments(new[] { "delete", serviceName })
                     .WithValidation(CommandResultValidation.ZeroExitCode);
        var rs = await cli.ExecuteBufferedAsync();
        return rs.ExitCode == 0;
    }

    /// <summary>
    /// 创建 Windows 服务
    /// </summary>
    /// <param name="binPath"></param>
    /// <param name="serviceName"></param>
    /// <param name="displayName"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static async Task<bool> CreateService(string binPath, string serviceName, string displayName = "", string description = "")
    {
        var args = string.IsNullOrEmpty(displayName)
                       ? new[] {"create", serviceName, "binpath=", $"{binPath}", "start= auto"}
                       : new[] {"create", serviceName, "binpath=", $"{binPath}", "displayname=", $"{displayName}", "start=", "auto"};
        var cli = Cli.Wrap($"sc.exe")
                     .WithArguments(args)
                     .WithValidation(CommandResultValidation.ZeroExitCode);
        await cli.ExecuteBufferedAsync();


        cli = Cli.Wrap($"sc.exe")
                 .WithArguments(new[] {"description", serviceName, description})
                 .WithValidation(CommandResultValidation.ZeroExitCode);
        var rs = await cli.ExecuteBufferedAsync();
        return rs.ExitCode == 0;
    }

    /// <summary>
    /// 获取运行状态
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public static async Task<RunningStatus> GetRunningStatus(string serviceName)
    {
        var cli = Cli.Wrap("sc.exe")
                     .WithArguments(new[] { "query", serviceName })
                     .WithValidation(CommandResultValidation.None);
        var rs = await cli.ExecuteBufferedAsync();

        if (rs.StandardOutput.IndexOf("STOPPED", StringComparison.OrdinalIgnoreCase) > 0) return RunningStatus.Stopped;
        if (rs.StandardOutput.IndexOf("RUNNING", StringComparison.OrdinalIgnoreCase) > 0) return RunningStatus.Running;
        return RunningStatus.UnKnown;
    }

    /// <summary>
    /// 是否安装
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public static async Task<bool> IsInstalled(string serviceName)
    {
        var cli = Cli.Wrap("sc.exe")
                     .WithArguments(new[] { "query", serviceName })
                     .WithValidation(CommandResultValidation.None);
        var rs = await cli.ExecuteBufferedAsync();

        return rs.ExitCode == 0;
    }

}