using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Assistant.Utils;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Assistant.Model.ServiceManager;

public static class ServiceBaseExtensions
{
    /// <summary>
    /// 打开服务安装目录
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task OpenDirectory(this ServiceBaseModel model)
    {
        ArgumentNullException.ThrowIfNull(model.ServiceDirectory, nameof(model.ServiceDirectory));
        await FileUtils.OpenDirectory(model.ServiceDirectory);
    }

    /// <summary>
    /// 打开配置文件
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task OpenConfigFile(this ServiceBaseModel model)
    {
        ArgumentNullException.ThrowIfNull(model.ConfigFilePath, nameof(model.ConfigFilePath));
        await FileUtils.OpenFileWithNotepad(model.ConfigFilePath);
    }

    /// <summary>
    /// 启动服务
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<bool> Start(this ServiceBaseModel model)
    {
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        return await WinServiceUtils.StartService(model.ServiceName);
    }

    /// <summary>
    /// 停止服务
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<bool> Stop(this ServiceBaseModel model)
    {
        ArgumentNullException.ThrowIfNull(model.ServiceName, nameof(model.ServiceName));
        return await WinServiceUtils.StopService(model.ServiceName);
    }
}