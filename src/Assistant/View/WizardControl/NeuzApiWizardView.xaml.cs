using System;
using Syncfusion.SfSkinManager;
using System.Windows;
using Assistant.Utils;
using Microsoft.Win32;
using Serilog;

namespace Assistant.View.WizardControl;

/// <summary>
/// NeuzApiWizardView.xaml 的交互逻辑
/// </summary>
public partial class NeuzApiWizardView
{
    public NeuzApiWizardView()
    {
        SfSkinManager.SetTheme(this, new Theme("FluentDark"));
        InitializeComponent();
    }

    private void WizardControl_OnNext(object sender, RoutedEventArgs e)
    {
        // if (!ConfigPage.IsSelected) return;
        //
        // try
        // {
        //     if (PdaPortTextBox.Value == null) throw new Exception($"PDA端口设置不允许为空");
        //     if (WebPortTextBox.Value == null) throw new Exception($"Web端口设置不允许为空");
        //     if (WebPortTextBox.Value == PdaPortTextBox.Value) throw new Exception($"Web端口和PDA端口不允许相同");
        //
        // }
        // catch (Exception ex)
        // {
        //     Log.Error(ex.Message);
        //     MessageBox.Show(ex.Message);
        // }
    }

    private void BtnOfd_OnClick(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog();

        if (ofd.ShowDialog(this) ?? false) TextBoxPackPath.Text = ofd.FileName;
    }


    private void BtnTest_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var rs  = MySQLUtils.TestConnect(TbMySQLHost.Text, TbMySQLPort.Text, TbMySQLUser.Text, TbMySQLPwd.Text);
            var msg = rs ? "连接成功" : "连接失败";
            MessageBox.Show(msg);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            var msg = $"连接失败\r\n{ex.Message}";
            MessageBox.Show(msg);
        }
    }
}