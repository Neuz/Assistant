using System;
using System.Windows.Controls;
using Assistant.ViewModel.BaseServices;

// ReSharper disable InconsistentNaming

namespace Assistant.View.BaseServices;

public partial class CaddyView : UserControl
{
    public CaddyView()
    {
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ((CaddyViewModel) DataContext).FlushCommand.Execute(null);
    }
}