using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System;
using Xilium.CefGlue.Common.Handlers;
using Xilium.CefGlue;
using Xilium.CefGlue.Avalonia;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using Xilium.CefGlue.Common.Composition;


namespace Minimal.BrowserUI;

public partial class BrowserView : UserControl
{
    private CefGlueBrowser browser;

    public BrowserView()
    {
        InitializeComponent();
        browser = CreateBrowser();
        browser.Address = MiniPipeline.Core.PipelineCefConfig.BaseAddress;
        browser.LoadStart += OnBrowserLoadStart;

        browser.TitleChanged += OnBrowserTitleChanged;
        browser.LifeSpanHandler = new BrowserLifeSpanHandler();
        browser.AddressChanged += (o, url) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                var addressTextBox = this.FindControl<TextBox>("addressTextBox");

                addressTextBox.Text = url;
            });

        };
        addressTextBox.KeyDown += OnAddressTextBoxKeyDown;

        browserWrapper.Child = browser.Control;

    }


    // From the app container (AddCefGlue in StartUp); a standalone browser only when there is none, e.g. the designer.
    private static CefGlueBrowser CreateBrowser()
    {
        var browser = MiniPipeline.Core.PipelineCefConfig.Services?.GetService<CefGlueHost>()?.CreateBrowser() ?? new CefGlueBrowser();
        // Lets the app's own pages reach its WebSocket listener (Chromium's Local Network Access prompt).
        browser.PermissionHandler = new MiniPipeline.CefGlue.PipelinePermissionHandler();
        return browser;
    }

    public event Action<string> TitleChanged;

    private void OnBrowserTitleChanged(object sender, string title)
    {
        TitleChanged?.Invoke(title);
    }

    private void OnBrowserLoadStart(object sender, Xilium.CefGlue.Common.Events.LoadStartEventArgs e)
    {
        if (e.Frame.Browser.IsPopup || !e.Frame.IsMain)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var addressTextBox = this.FindControl<TextBox>("addressTextBox");

            //addressTextBox.Text = e.Frame.Url;
        });
    }

    private void OnAddressTextBoxKeyDown(object sender, global::Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            browser.Address = ((TextBox)sender).Text;
        }
    }


    public void OpenDevTools()
    {
        browser.ShowDeveloperTools();
    }

    public void Dispose()
    {
        browser.Dispose();
    }

    private class BrowserLifeSpanHandler : LifeSpanHandler
    {
        protected override bool OnBeforePopup(
            CefBrowser browser,
            CefFrame frame,
            int popupId,
            string targetUrl,
            string targetFrameName,
            CefWindowOpenDisposition targetDisposition,
            bool userGesture,
            CefPopupFeatures popupFeatures,
            CefWindowInfo windowInfo,
            ref CefClient client,
            CefBrowserSettings settings,
            ref CefDictionaryValue extraInfo,
            ref bool noJavascriptAccess)
        {
            var bounds = windowInfo.Bounds;
            Dispatcher.UIThread.Post(() =>
            {
                var window = new Window();
                var popupBrowser = CreateBrowser();
                popupBrowser.Address = targetUrl;
                window.Content = popupBrowser.Control;
                window.Closed += (_, _) => popupBrowser.Dispose();
                window.Position = new PixelPoint(bounds.X, bounds.Y);
                window.Height = bounds.Height;
                window.Width = bounds.Width;
                window.Title = targetUrl;
                window.Show();
            });
            return true;
        }
    }


}
