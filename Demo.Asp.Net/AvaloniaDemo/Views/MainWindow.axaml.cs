using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaDemo.Behaviors;
using AvaloniaDemo;
using AvaloniaDemo.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Material.Styles.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;

namespace AvaloniaDemo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
       

    }



    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        TabScrollBehavior.Attach(simplechrometabs);

    }



}
