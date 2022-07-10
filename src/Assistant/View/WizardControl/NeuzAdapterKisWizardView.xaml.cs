using System;
using Syncfusion.SfSkinManager;
using System.Windows;
using System.Windows.Media;
using Assistant.Utils;
using Microsoft.Win32;
using Serilog;

namespace Assistant.View.WizardControl;

/// <summary>
/// RedisWizardView.xaml 的交互逻辑
/// </summary>
public partial class NeuzAdapterKisWizardView
{
    public NeuzAdapterKisWizardView()
    {
        SfSkinManager.SetTheme(this, new Theme("FluentDark"));
        InitializeComponent();
    }

    private void WizardControl_OnNext(object sender, RoutedEventArgs e)
    {
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

    private void BtnOfd_OnClick(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog {Filter = "压缩文件(*.zip)|*.zip|所有文件(*.*)|*.*"};

        if (ofd.ShowDialog(this) ?? false) TbZipFilePath.Text = ofd.FileName;
    }
}