using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System;
using Xilium.CefGlue.Common.Handlers;
using Xilium.CefGlue;
using Xilium.CefGlue.Avalonia;
using Avalonia.Input;


namespace Minimal.BrowserUI;

public partial class BrowserView : UserControl
{
    private AvaloniaCefBrowser browser;

    public BrowserView()
    {
        InitializeComponent();
        browser = new AvaloniaCefBrowser();
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

        browserWrapper.Child = browser;

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
                var popupBrowser = new AvaloniaCefBrowser();
                popupBrowser.Address = targetUrl;
                window.Content = popupBrowser;
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
