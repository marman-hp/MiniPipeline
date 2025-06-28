using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaDemo.Handler;
using AvaloniaDemo.ViewModels;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Xilium.CefGlue.Avalonia;

namespace AvaloniaDemo.Views
{

    public partial class BrowserView : UserControl
    {

        private AvaloniaCefBrowser browser;
        BrowserViewModel? viewModel = null;
        public BrowserView()
        {
            InitializeComponent();


            this.Initialized += (s, e) =>
            {
                if (!Design.IsDesignMode)
                {
                    viewModel = DataContext as BrowserViewModel;
                    browser = new AvaloniaCefBrowser();
                    browser.DisplayHandler = new SimpleDisplayHandler();
                    browser.LifeSpanHandler = new SimpleLifeSpanHandler();
                    browser.ContextMenuHandler = new SimpleContextMenu();
                    browser.DownloadHandler = new SimpleDownloadHandler(() =>
                    {
                        // Platform-specific download path logic
                        string downloadsPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Downloads")
                        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                        return Path.GetFullPath(downloadsPath);
                    });

                    if (viewModel?.Autolunch == true)
                        viewModel?.AttachBrowser(browser);

                    ((SimpleDisplayHandler)browser.DisplayHandler).FavoIconChanged += (s, bytes) =>
                    {
                        if (bytes == null || bytes.Length == 0)
                        {
                            viewModel.Favicon = null;
                            return;
                        }

                        try
                        {
                            using var ms = new MemoryStream(bytes);
                            viewModel.Favicon = new Bitmap(ms);
                        }
                        catch
                        {
                            viewModel.Favicon = null;
                        }
                    };

                    browser.Address = viewModel?.Url;
                    browser.LoadingStateChange += (o, e) =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            viewModel.IsBusy = e.IsLoading;  
                            if (!e.IsLoading)
                            {
                                viewModel.Progress = 0;  
                            }
                        });
                    };



                    browser.LoadingStateChange += (o, e) =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            viewModel!.CanGoForward = e.CanGoForward;
                            viewModel!.CanGoBack = e.CanGoBack;
                        });
                    };

                    browser.AddressChanged += (o, url) => viewModel!.Url = url;
                    browser.TitleChanged += (o, title) => viewModel!.Title = string.IsNullOrEmpty(title) ? viewModel!.Url : title;


                    browser.Settings.DefaultEncoding = "charset: utf-8";
                    var decorator = this.FindControl<Decorator>("browserWrapper");
                    decorator!.Child = browser;



                }
            };
        }

    }

}