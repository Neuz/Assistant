using Syncfusion.SfSkinManager;
using System.Windows;
using Microsoft.Win32;

namespace Assistant.View.WizardControl;

/// <summary>
/// NeuzAdapterK3WiseWizardView.xaml 的交互逻辑
/// </summary>
public partial class NeuzAdapterK3WiseWizardView
{
    public NeuzAdapterK3WiseWizardView()
    {
        SfSkinManager.SetTheme(this, new Theme("FluentDark"));
        InitializeComponent();
    }

    private void WizardControl_OnNext(object sender, RoutedEventArgs e)
    {
    }

    private void BtnOfd_OnClick(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog {Filter = "压缩文件(*.zip)|*.zip|所有文件(*.*)|*.*"};

        if (ofd.ShowDialog(this) ?? false) TbZipFilePath.Text = ofd.FileName;
    }
}