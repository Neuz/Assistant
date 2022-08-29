using System;
using System.Windows;
using System.Windows.Media;
using Assistant.Utils;
using Serilog;
using Syncfusion.SfSkinManager;

namespace Assistant.View.ServiceWizard;

/// <summary>
/// NginxWizardView.xaml 的交互逻辑
/// </summary>
public partial class NginxWizardView
{
    public NginxWizardView()
    {
        SfSkinManager.SetTheme(this, new Theme("FluentDark"));
        InitializeComponent();
    }
    
    private async void BtnTest_OnClick(object sender, RoutedEventArgs e)
    {
        BusyIndicator.IsBusy = true;

        try
        {
            if (Convert.ToInt32(WebPortTextBox.Value) == Convert.ToInt32(PdaPortTextBox.Value))
            {
                throw new Exception($"Web端口与PDA端口相同");
            }

            if (await WinServiceUtils.IsInstalled(TbServiceName.Text))
                throw new Exception($"[{TbServiceName.Text}] 服务名已存在");

            if (NetWorkUtils.PortAvailable(Convert.ToInt32(WebPortTextBox.Value)))
                throw new Exception($"[{WebPortTextBox.Value}] 端口已使用");

            if (NetWorkUtils.PortAvailable(Convert.ToInt32(PdaPortTextBox.Value)))
                throw new Exception($"[{PdaPortTextBox.Value}] 端口已使用");

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