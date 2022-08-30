using System;
using System.Windows;
using System.Windows.Media;
using Assistant.Messages;
using Assistant.Utils;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using Syncfusion.SfSkinManager;

namespace Assistant.View.BackupWizard;

/// <summary>
/// RedisWizardView.xaml 的交互逻辑
/// </summary>
public partial class BackupWizardView
{
    public BackupWizardView()
    {
        SfSkinManager.SetTheme(this, new Theme("FluentDark"));


        InitializeComponent();
        WizardControl.SelectedPageChanged += WizardControl_SelectedPageChanged;
    }

    private void WizardControl_SelectedPageChanged(object sender, RoutedEventArgs e)
    {
        if (WizardControl.SelectedWizardPage.Name == "PageTables") 
        {
            WeakReferenceMessenger.Default.Send<LoadTablesMessage>(); // 加载数据表
        }
    }

    private async void TestBtn_Click(object sender, RoutedEventArgs e)
    {
        BusyIndicator.IsBusy = true;
        try
        {
            var rs = await MySQLUtils.TestConnect(TbHost.Text, TbPort.Text, TbUser.Text, TbPwd.Text);
            TbTestResult.Text       = rs ? "连接成功" : "连接失败";
            TbTestResult.Foreground = new SolidColorBrush(Colors.GreenYellow);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            TbTestResult.Text       = $"连接失败\r\n{ex.Message}";
            TbTestResult.Foreground = new SolidColorBrush(Colors.Red);
        }

        WeakReferenceMessenger.Default.Send<LoadDatabaseMessage>(); // 加载数据库


        BusyIndicator.IsBusy = false;  
    }
}