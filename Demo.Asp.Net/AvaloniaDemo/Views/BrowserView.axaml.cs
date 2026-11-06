using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaDemo.Handler;
using AvaloniaDemo.ViewModels;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Composition;

namespace AvaloniaDemo.Views
{

    public partial class BrowserView : UserControl
    {

        private CefGlueBrowser browser;
        BrowserViewModel? viewModel = null;
        public BrowserView()
        {
            InitializeComponent();


            this.Initialized += (s, e) =>
            {
                if (!Design.IsDesignMode)
                {
                    viewModel = DataContext as BrowserViewModel;
                    var options = new CefGlueBrowserOptions
                    {
                        SetupCefBrowserSettings = () => new Xilium.CefGlue.CefBrowserSettings { DefaultEncoding = "utf-8" },
                    };
                    // From the app container (AddCefGlue in StartUp), so the browser shares the app's services.
                    var host = MiniPipeline.Core.PipelineCefConfig.Services?.GetService<CefGlueHost>();
                    browser = host?.CreateBrowser(options) ?? new CefGlueBrowser(setupCefBrowserSettings: options.SetupCefBrowserSettings);
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


                    var decorator = this.FindControl<Decorator>("browserWrapper");
                    decorator!.Child = browser.Control;



                }
            };
        }

    }

}