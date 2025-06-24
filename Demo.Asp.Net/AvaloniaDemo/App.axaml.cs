using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaDemo.ViewModels;
using AvaloniaDemo.Views;
using MiniPipeline.Core;
using System;
using System.Diagnostics;

namespace AvaloniaDemo;

public partial class App : Application
{
    private bool _isDownloadWindowOpened = false;
    private DownloadManagerWindow? _downloadManagerWindow;
    private DownloadManagerViewModel? _downloadManagerViewModel;
    MainViewModel? simpleTabChromeModel ;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
         
            desktop.MainWindow.Focus();
            desktop.MainWindow.Closed += (_, _) =>
            {
                _downloadManagerWindow?.Close(); 
            };
            simpleTabChromeModel = new MainViewModel();
            simpleTabChromeModel.AddNewTab($"{PipelineCefConfig.BaseAddress}"); 

            //create another tab for testing
            simpleTabChromeModel.AddNewTab("https://google.com");

            desktop.MainWindow.DataContext = simpleTabChromeModel;

            //desktop.MainWindow.Show();
            CreateDownloadManager();


        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void AddTab(string? url=null, bool autolunch = true)
    {
        simpleTabChromeModel?.AddNewTab(url);
    }

    public void ShowDevTools()
    {
        simpleTabChromeModel?.SelectedTab.ShowDevTools();
    }


    private readonly object _downloadLock = new();

    public void CreateDownloadManager()
    {
        _downloadManagerWindow = new DownloadManagerWindow()
        {
            ShowInTaskbar = false,
            Title = "Simple demo download manager for testing purpose!"
        };
        _downloadManagerViewModel = new DownloadManagerViewModel();
        _downloadManagerWindow.DataContext = _downloadManagerViewModel;

        _downloadManagerWindow.Closing += (o,e) =>
        {  
            _isDownloadWindowOpened = false;
  
        };
    }
    public void ShowDownloadManager()
    {
        Debug.WriteLine($"[DEBUG] ShowDownloadManager, current window: {_downloadManagerWindow?.GetHashCode()}");
        if (!_isDownloadWindowOpened )
        {
            _isDownloadWindowOpened = true;
             _downloadManagerWindow.Show();
        }
    }

    public void AddDownloadInfo(uint id, string url,string suggestFileName, long totalBytes)
    {
        _downloadManagerViewModel?.Add(id,   url, suggestFileName, totalBytes);
    }

    public void UpdateDownloadInfo(uint id, long receivedBytes, long totalBytes, bool isComplete, bool isCanceled, string reason)
    {
        _downloadManagerViewModel?.Update(id,   receivedBytes, totalBytes, isComplete, isCanceled,reason);
    }

}
