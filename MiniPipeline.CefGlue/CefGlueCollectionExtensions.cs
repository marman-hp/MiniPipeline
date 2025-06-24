using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xilium.CefGlue.Common.Shared;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;

using MiniPipeline.Core;
using MiniPipeline.WebSocket;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;

namespace MiniPipeline.CefGlue
{
    public static class CefGlueCollectionExtensions
    {
        internal static bool ContainsEndpointMiddleware(IApplicationBuilder app)
        {
            var field = app.GetType()
                .GetField("_components", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                return false;
            }

            var components = field.GetValue(app) as IList<Func<RequestDelegate, RequestDelegate>>;
            if (components == null)
            {
                return false;
            }

            foreach (var component in components)
            {
                var target = component.Target;
                if (target != null)
                {
                    var targetType = target.GetType().FullName;
                    if (targetType?.Contains("EndpointMiddleware") == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public static void CefInitSchemeHandler(this IApplicationBuilder app, CefSettings settings, Dictionary<string, string>? cefArgs = null)
        {
            try
            {
                if (!ContainsEndpointMiddleware(app))
                {
                    app.UseEndpoints(_ => { });
                }
            }
            catch
            { }

            var factory = new PipeSchemeHandlerFactory(app);
            var endpointDataSource = app.ApplicationServices.GetRequiredService<EndpointDataSource>();


            foreach (var ep in endpointDataSource.Endpoints)
            {
                if (ep is RouteEndpoint routeEp && routeEp.RoutePattern.RawText.Contains("blazor"))
                {
                    app.ApplicationServices.GetService<IStore>().Set("__isblazor", true);
                    break;
                }
            }


            PipelineCefConfig.baseAddress = $"{PipelineCefConfig.Scheme}://{PipelineCefConfig.Host}";
            var env = app.ApplicationServices.GetService<IWebHostEnvironment>();
            PipelineCefConfig.wwwrootfolder = Helper.GetWebRootPath(env);

            if (app.ApplicationServices.GetService<IBackgroundStatus>() != null)
            {
                var options = app.ApplicationServices.GetRequiredService<IOptions<PipelineSocketOptions>>()?.Value;

                PipelineCefConfig.Scheme = options.UseSsl ? "https" : "http";

                try
                {

                    IPHostEntry hostEntry = Dns.GetHostEntry(options.IPAddress);
                    PipelineCefConfig.Host = options.IPAddress.ToString().Equals("127.0.0.1") ? "localhost" : hostEntry.HostName;
                    PipelineCefConfig.baseAddress = $"{PipelineCefConfig.Scheme}://{PipelineCefConfig.Host}:{options.Port}";

                    (app as IHost)?.StartAsync();

                }
                catch (SocketException ex)
                {
                    throw new Exception("the IP address you provide need a hostname.");
                }

            }

            settings.CookieableSchemesList = PipelineCefConfig.Scheme;

            var custome = new CustomScheme()
            {
                SchemeName = PipelineCefConfig.Scheme,
                DomainName = PipelineCefConfig.Host,
                SchemeHandlerFactory = factory
            };

            CefRuntimeLoader.Initialize(settings, customSchemes: new[] { custome });
        }

        public static void CefShutdown(this IApplicationBuilder app)
        {
            var status = app.ApplicationServices.GetService<IBackgroundStatus>();
            if (status != null && status.IsRunning)
            {
                (app as IHost)?.StopAsync();
            }
            CefRuntime.Shutdown();
        }




    }
}
