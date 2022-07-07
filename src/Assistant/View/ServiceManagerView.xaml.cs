using System;
using System.Windows.Controls;

namespace Assistant.View;

/// <summary>
/// ServiceManagerView.xaml 的交互逻辑
/// </summary>
public partial class ServiceManagerView : UserControl
{
    public ServiceManagerView()
    {
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        BtnFlush.Command.Execute(null);
    }
}