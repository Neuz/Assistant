using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Assistant.Messages;
using Assistant.View.BaseServices;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Assistant.ViewModel;

public partial class DashboardViewModel : ObservableObject
{

    public string Title => "仪表盘";
    public Geometry? Icon { get; } = Application.Current.FindResource("Svg.Sql") as Geometry;
    public string IconColor => "#FAD83B";


    [RelayCommand]
    private void Flush()
    {
        // todo
        
        WeakReferenceMessenger.Default.Send(new GotoMessage<Type>(typeof(MySQLView)));
    }
}
