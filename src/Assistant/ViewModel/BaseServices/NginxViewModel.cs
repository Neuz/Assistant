using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Assistant.Model.BaseServices;
using Assistant.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

// ReSharper disable InconsistentNaming

namespace Assistant.ViewModel.BaseServices;

public partial class NginxViewModel : ObservableObject
{
    public NginxViewModel()
    {
    }

    private readonly UIService _uiService = Ioc.Default.GetRequiredService<UIService>();


    public string Title => "Nginx";
    public Geometry? Icon { get; } = Application.Current.FindResource("Svg.Sql") as Geometry;
    public string Version => "v5.7.36";

    [ObservableProperty]
    private MySQL _mySql = new();

    [RelayCommand]
    private void Flush()
    {
    }
}