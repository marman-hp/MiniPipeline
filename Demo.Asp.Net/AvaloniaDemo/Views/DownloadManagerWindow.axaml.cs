using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaDemo.ViewModels;
using System;
using System.Diagnostics;

namespace AvaloniaDemo;

public partial class DownloadManagerWindow : Window
{
    public DownloadManagerWindow()
    {

        InitializeComponent();
         Debug.WriteLine($"[DEBUG] DownloadManagerWindow created: {GetHashCode()}");
    }
}