using System.Diagnostics;
using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace AvaloniaDemo.Handler
{
    public class SimpleLifeSpanHandler : LifeSpanHandler
    {
    


    
        protected override bool OnBeforePopup(CefBrowser browser,
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
             browser.Dispose();
             (Avalonia.Application.Current as App)?.AddTab(targetUrl);
            return true;
        }
    }

}