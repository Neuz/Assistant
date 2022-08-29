using System;
using System.Windows;
using System.Windows.Media;
using Assistant.Utils;
using Microsoft.Win32;
using Serilog;
using Syncfusion.SfSkinManager;

namespace Assistant.View.ServiceWizard;

/// <summary>
/// NeuzAppWizardView.xaml 的交互逻辑
/// </summary>
public partial class NeuzAppWizardView
{
    public NeuzAppWizardView()
    {
        SfSkinManager.SetTheme(this, new Theme("FluentDark"));
        InitializeComponent();
    }

    private async void BtnTest_OnClick_1(object sender, RoutedEventArgs e)
    {
        BusyIndicator1.IsBusy = true;

        try
        {
            if (await WinServiceUtils.IsInstalled(TbServiceName.Text))
                throw new Exception($"[{TbServiceName.Text}] 服务名已存在");

            if (NetWorkUtils.PortAvailable(Convert.ToInt32(TbPort.Value)))
                throw new Exception($"[{TbPort.Value}] 端口已使用");

            TbTestResult1.Text       = "通过";
            TbTestResult1.Foreground = new SolidColorBrush(Colors.GreenYellow);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            TbTestResult2.Text       = $"失败\r\n{ex.Message}";
            TbTestResult2.Foreground = new SolidColorBrush(Colors.Red);
        }

        BusyIndicator1.IsBusy = false;
    }

    private async void BtnTest_OnClick_2(object sender, RoutedEventArgs e)
    {
        BusyIndicator2.IsBusy = true;

        try
        {
            var rs = await MySQLUtils.TestConnect(TbHost.Text, TbMySqlPort.Text, TbUser.Text, TbPassword.Text);
            TbTestResult2.Text       = rs ? "连接成功" : "连接失败";
            TbTestResult2.Foreground = new SolidColorBrush(Colors.GreenYellow);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            TbTestResult2.Text       = $"连接失败\r\n{ex.Message}";
            TbTestResult2.Foreground = new SolidColorBrush(Colors.Red);
        }

        BusyIndicator2.IsBusy = false;
    }

    private void BtnOfd_OnClick(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog {Filter = "压缩文件(*.zip)|*.zip|所有文件(*.*)|*.*"};

        if (ofd.ShowDialog(this) ?? false) TbZipFilePath.Text = ofd.FileName;
    }
}