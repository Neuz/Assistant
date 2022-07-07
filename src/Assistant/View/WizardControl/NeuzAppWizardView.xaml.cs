using Assistant.Utils;
using Microsoft.Win32;
using Serilog;
using Syncfusion.SfSkinManager;
using System;
using System.Windows;
using System.Windows.Media;

namespace Assistant.View.WizardControl;

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

    private void WizardControl_OnNext(object sender, RoutedEventArgs e)
    {
    }

    private async void BtnTest_OnClick(object sender, RoutedEventArgs e)
    {
        BusyIndicator.IsBusy = true;

        try
        {
            var rs = await MySQLUtils.TestConnect(TbHost.Text, TbPort.Text, TbUser.Text, TbPassword.Text);
            TbTestResult.Text       = rs ? "连接成功" : "连接失败";
            TbTestResult.Foreground = new SolidColorBrush(Colors.GreenYellow);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            TbTestResult.Text       = $"连接失败\r\n{ex.Message}";
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