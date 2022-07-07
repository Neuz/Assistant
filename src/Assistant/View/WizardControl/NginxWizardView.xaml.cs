using Syncfusion.SfSkinManager;
using System.Windows;

namespace Assistant.View.WizardControl;

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
}