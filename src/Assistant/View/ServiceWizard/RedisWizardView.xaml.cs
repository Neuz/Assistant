﻿using System;
using System.Windows;
using System.Windows.Media;
using Assistant.Utils;
using Serilog;
using Syncfusion.SfSkinManager;

namespace Assistant.View.ServiceWizard;

/// <summary>
/// RedisWizardView.xaml 的交互逻辑
/// </summary>
public partial class RedisWizardView
{
    public RedisWizardView()
    {
        SfSkinManager.SetTheme(this, new Theme("FluentDark"));
        InitializeComponent();
    }

    private async void BtnTest_OnClick(object sender, RoutedEventArgs e)
    {
        BusyIndicator.IsBusy = true;

        try
        {
            if (await WinServiceUtils.IsInstalled(TbServiceName.Text))
                throw new Exception($"[{TbServiceName.Text}] 服务名已存在");

            if (NetWorkUtils.PortAvailable(Convert.ToInt32(TbPort.Value)))
                throw new Exception($"[{TbPort.Value}] 端口已使用");

            TbTestResult.Text       = "通过";
            TbTestResult.Foreground = new SolidColorBrush(Colors.GreenYellow);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            TbTestResult.Text       = $"失败\r\n{ex.Message}";
            TbTestResult.Foreground = new SolidColorBrush(Colors.Red);
        }

        BusyIndicator.IsBusy = false;
    }
}