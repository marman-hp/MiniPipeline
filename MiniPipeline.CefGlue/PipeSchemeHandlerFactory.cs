using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MiniPipeline.Core;
using MiniPipeline.WebSocket;
using System.Diagnostics;
using Xilium.CefGlue;
using Xilium.CefGlue.Common.Handlers;

namespace MiniPipeline.CefGlue
{
    public class PipeSchemeHandlerFactory : CefSchemeHandlerFactory
    {
        readonly PipelineSocketOptions? options;
        readonly IApplicationBuilder _app;
        public PipeSchemeHandlerFactory( IApplicationBuilder app) 
        {
            var store = app.ApplicationServices.GetRequiredService(typeof(IStore)) as IStore;
            store.Build(app);
            _app = app;
        }

        protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
        {
            var options = (_app.ApplicationServices.GetService(typeof(IOptions<PipelineSocketOptions>)) as IOptions<PipelineSocketOptions>)?.Value;

            var status = _app.ApplicationServices.GetService(typeof(IBackgroundStatus)) != null;

            if(status)
            {
                Uri url = new Uri(request.Url);
                if(url.Port != options.Port) // protect the port
                {
                    var notfound = new DefaultResourceHandler();
                    notfound.Status =  404;
                    notfound.StatusText = Helper.GetStatus(404);
                    return notfound;
                }
            } 
             return new PipeSchemeHandler(_app!.ApplicationServices); 
        }


    }

}
