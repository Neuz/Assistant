using System;
using System.Windows.Controls;
using Assistant.ViewModel.BaseServices;

// ReSharper disable InconsistentNaming

namespace Assistant.View.BaseServices;

/// <summary>
/// MySQLView.xaml 的交互逻辑
/// </summary>
public partial class RedisView : UserControl
{
    public RedisView()
    {
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ((RedisViewModel) DataContext).FlushCommand.Execute(null);
    }
}