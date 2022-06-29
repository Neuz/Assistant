using Assistant.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Assistant.View;

/// <summary>
/// LogView.xaml 的交互逻辑
/// </summary>
public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        lock (MainViewModel.LogSyncLock)
        {
            RichTextBox.Document.Blocks.Clear();
        }
    }
}