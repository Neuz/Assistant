using System.Windows;
using System.Windows.Controls;
using Assistant.ViewModel;

namespace Assistant.View;

/// <summary>
/// BackupView.xaml 的交互逻辑
/// </summary>
public partial class BackupView : UserControl
{
    public BackupView()
    {
        InitializeComponent();
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        ((BackupViewModel) DataContext).RestoreBackupFileCommand.Execute(null);
    }
}