using System;
using System.Collections.Generic;
using System.Linq;
using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace AvaloniaDemo.Handler
{
    public class SimpleDisplayHandler : DisplayHandler
    {
        public delegate void OnFavoIconChanged(object sender,byte[] bytes);
        public event OnFavoIconChanged FavoIconChanged ; 
        public SimpleDisplayHandler() { }
        protected override void OnFaviconUrlChange(CefBrowser browser, string[] iconUrls)
        {
            base.OnFaviconUrlChange(browser, iconUrls);
            if (iconUrls != null && iconUrls.Count() > 0)
            {
                var faviconUrl = iconUrls[0];

                // Now, instead of HttpClient, we download it via CEF itself
                browser.GetHost().DownloadImage(
                    faviconUrl,
                    true,
                    maxImageSize: 32,
                    bypassCache: false,
                    new FaviconDownloadCallback((bytes) => FavoIconChanged?.Invoke(this,bytes))
                );
            }
        }
    }

    public class FaviconDownloadCallback : CefDownloadImageCallback
    {
        private readonly Action<byte[]> _onFaviconReady;

        public FaviconDownloadCallback(Action<byte[]> onFaviconReady)
        {
            _onFaviconReady = onFaviconReady;
        }



        protected override void OnDownloadImageFinished(string imageUrl, int httpStatusCode, CefImage image)
        {
            if (image != null && !image.IsEmpty)
            {
                // Cek beberapa faktor skala dan ambil yang terbaik
                var scaleFactors = new List<float> { 1.0f, 1.25f, 1.5f, 2.0f, 3.0f, 4.0f };
                float bestScale = 1.0f;
                int bestWidth = 0;
                int bestHeight = 0;
                CefBinaryValue? bestImage = null;

                foreach (var scale in scaleFactors)
                {
                    var pngBytes = image.GetAsPng(scale, true, out int width, out int height);
                    if (pngBytes != null && width * height > bestWidth * bestHeight)
                    {
                        bestScale = scale;
                        bestWidth = width;
                        bestHeight = height;
                        bestImage = pngBytes;
                    }
                }

                if (bestImage != null)
                {
                    _onFaviconReady?.Invoke(bestImage?.ToArray());
                }
                else
                {
                    _onFaviconReady?.Invoke(null);
                }
            }
            else
            {
                _onFaviconReady?.Invoke(null);
            }
        }
    }


}
