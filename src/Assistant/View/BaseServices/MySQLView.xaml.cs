using System;
using System.Windows.Controls;
using Assistant.ViewModel.BaseServices;

// ReSharper disable InconsistentNaming

namespace Assistant.View.BaseServices;

public partial class MySQLView : UserControl
{
    public MySQLView()
    {
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ((MySQLViewModel) DataContext).FlushCommand.Execute(null);
    }
}