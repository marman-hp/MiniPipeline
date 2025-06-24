using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using MiniPipeline.WebSocket;

namespace MiniPipeline.Core
{

    public static class MiniPipelineBuilder
    {
        public static WebApplicationBuilder CreateBuilder()
        {
            var builder = WebApplication.CreateBuilder();
            Inititalize(builder);
            return builder;
        }

        public static WebApplicationBuilder CreateBuilder(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Inititalize(builder);
            return builder;
        }

        public static WebApplicationBuilder CreateBuilder(WebApplicationOptions options)
        {
            var builder = WebApplication.CreateBuilder(options);
            Inititalize(builder);
            return builder;
        }



        private static void Inititalize(WebApplicationBuilder builder)
        {
            builder.WebHost.UseSetting("UseKestrel", "false");
            builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

            // Remove existing IServer (usually Kestrel)
            var descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IServer));
            if (descriptor != null)
            {
                builder.Services.Remove(descriptor);

            }

            builder.Services.AddSingleton<IServer, NoServer>();

            
            builder.Services.TryAddSingleton<IStore>( new PipelineStore());

            builder.Services.TryAddSingleton<ObjectPool<MemoryStream>>(sp =>
            {
                var provider = new DefaultObjectPoolProvider();
                return provider.Create(new MemoryStreamPooledObjectPolicy());
            });

            builder.Services.TryAddScoped<IHttpContextPipeline, HttpContextPipeline>();

            builder.Services.TryAddScoped<IPipelineFeaturesFactory, DefaultPipelineFeaturesFactory>();

            builder.Services.TryAddScoped<IPipelineProcessor, DefaultPipelineProcessor>();

            builder.Services.AddScoped<IPipelineAddon, InlineSourceMapInjector>();


        }

    }
    public static class PipelineCollectionExtensions
    {
      

        public static IServiceCollection AddSocketPipeline(this IServiceCollection services)
        {
            services.AddScoped<IPipelineAddon, SupportNegotiation>();
            services.TryAddSingleton<IClientHandler, PipelineClientHandler>();
            services.TryAddSingleton<IHttpRequestParser, HttpRequestParser>(); 
            services.TryAddSingleton<IWebSocketRequestValidator, WebSocketRequestValidator>(); 
            services.TryAddSingleton<ISocketFeaturesFactory, DefaultSocketFeaturesFactory>();
            services.TryAddSingleton<ITcpListener, PipelineTcpListener>();
            services.AddSingleton<IBackgroundStatus, PipelineSocketService>();
            services.AddHostedService<PipelineSocketService>();
            
            return services;
        }



    }
}
