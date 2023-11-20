using Assistant.Model.OperationsTools;
using Syncfusion.Data.Extensions;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using System.Xml.Linq;
using CommunityToolkit.Diagnostics;
// ReSharper disable ReplaceWithSingleCallToAny

namespace Assistant.Services;

public class WinSvcService
{
    /// <summary>
    /// 是否已经注册
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool IsInstalled(string name)
    {
       return ServiceController.GetServices()
                         .Where(s => s.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase))
                         .Any();
    }

    /// <summary>
    /// 查询服务
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public List<WinSvc?> Query(string? name = null)
    {
        var services = name.IsNullOrWhiteSpace()
                           ? ServiceController.GetServices()
                           : ServiceController.GetServices().Where(s => s.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase));
        var rs = services.Select(s => new WinSvc
                          {
                              ServiceName        = s.ServiceName,
                              DisplayName = s.DisplayName,
                              Description = string.Empty,
                              Pid         = null,
                              BinPath     = null,
                              StartType   = (WinSvc.StartMode) s.StartType,
                              Status      = (WinSvc.RunningStatus) s.Status
                          })
                         .ToList();
        GetSvcInfo(rs);

        return rs;
    }



    private void GetSvcInfo(IEnumerable<WinSvc> services)
    {
        foreach (var s in services)
        {
            var mo = new ManagementObject($"Win32_Service.Name='{s.ServiceName}'");
            mo.Get();
            s.Description = mo.GetPropertyValue("Description")?.ToString();
            s.Pid = mo.GetPropertyValue("ProcessId") != null
                        ? Convert.ToInt32(mo.GetPropertyValue("ProcessId"))
                        : null;
            s.BinPath = mo.GetPropertyValue("PathName")?.ToString();
        }
    }

    /// <summary>
    /// 启动服务
    /// </summary>
    /// <param name="svc"></param>
    /// <returns></returns>
    public async Task<WinSvc> Start(WinSvc svc)
    {
        Guard.IsNotNullOrWhiteSpace(svc.ServiceName);
        var rs = await Start(svc.ServiceName);
        return Query(svc.ServiceName).First();
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
    /// <param name="svc"></param>
    /// <returns></returns>
    public async Task<WinSvc> Stop(WinSvc svc)
    {
        Guard.IsNotNullOrWhiteSpace(svc.ServiceName);
        var rs = await Stop(svc.ServiceName);
        return Query(svc.ServiceName).First();
    }

    /// <summary>
    /// 停止服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public async Task<BufferedCommandResult> Stop(string serviceName)
    {
        var cli = Cli.Wrap("net")
                     .WithArguments(new[] { "stop", serviceName });
        return await cli.ExecuteBufferedAsync();
    }

    /// <summary>
    /// 卸载服务
    /// </summary>
    /// <param name="svc"></param>
    /// <returns></returns>
    public async Task<WinSvc> Delete(WinSvc svc)
    {
        Guard.IsNotNullOrWhiteSpace(svc.ServiceName);
        var rs = await Delete(svc.ServiceName);
        return Query(svc.ServiceName).First();
    }

    /// <summary>
    /// 卸载服务
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public async Task<BufferedCommandResult> Delete(string serviceName)
    {
        var cli = Cli.Wrap("sc")
                     .WithArguments(new[] { "delete", serviceName })
                     .WithValidation(CommandResultValidation.None);
        return await cli.ExecuteBufferedAsync();
    }
}