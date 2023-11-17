using Syncfusion.SfSkinManager;

namespace Assistant;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView
{
    public MainView()
    {
        SfSkinManager.SetTheme(this, new Theme("FluentDark"));
        InitializeComponent();
    }
}