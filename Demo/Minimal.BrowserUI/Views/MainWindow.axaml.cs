using Avalonia.Controls;

namespace Minimal.BrowserUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
    }
    BrowserView browserView;
    private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        browserView = new BrowserView();
        browserView.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        browserView.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        MainContainer.Children.Add(browserView);
        MenuDevtool.Click += MenuDevtool_Click;
        
    }

    private void MenuDevtool_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        browserView.OpenDevTools();
    }
}
