using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Assistant.ViewModel;

public class DashboardViewModel : ObservableObject
{
    public string Title => "仪表盘";
    public Geometry? Icon { get; } = Application.Current.FindResource("Svg.Sql") as Geometry;
    public string IconColor => "#FAD83B";
    public string Version => "v5.7.36";
}