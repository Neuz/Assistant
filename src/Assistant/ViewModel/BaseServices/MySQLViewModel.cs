using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;

// ReSharper disable InconsistentNaming

namespace Assistant.ViewModel.BaseServices;

public class MySQLViewModel : ObservableObject
{
    public string Title => "MySQL";
    public Geometry? Icon { get; } = Application.Current.FindResource("Svg.Sql") as Geometry;
    public string IconColor => "#FAD83B";
    public string Version => "v5.7.36";

    
}