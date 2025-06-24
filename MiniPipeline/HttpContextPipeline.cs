using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace MiniPipeline.Core
{
    public class HttpContextPipeline : IHttpContextPipeline
    {
        private readonly string _safeScheme;
        private readonly string _safeHost;

        private readonly Lazy<DefaultHttpContextFactory> _contextFactory;

        private readonly ILogger? _logger;
        private readonly Lazy<IStore> _store;
        public HttpContextPipeline(ILoggerFactory loggerFactory, IServiceProvider sp)
        {
            _safeScheme = PipelineCefConfig.Scheme;
            _safeHost = PipelineCefConfig.Host;

            _contextFactory = new Lazy<DefaultHttpContextFactory>(() => new DefaultHttpContextFactory(sp));
            _logger = loggerFactory?.CreateLogger<HttpContextPipeline>() ?? throw new InvalidOperationException("ILoggerFactory is not available.");
            _store = new Lazy<IStore>(() => sp.GetService<IStore>());
        }

        public Func<HttpContext,  Task> SocketInvokeAsync => async (context) =>
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context), "HttpContext is null");

             await _store.Value.Invoke(context);
        };
       
       [EditorBrowsable(EditorBrowsableState.Never)]
       [Bindable(false)]
       internal IStore Store => _store.Value;

        public Func<HttpContext, CancellationToken, Task> InvokeAsync => async (context, cancellationToken) =>
        {
            try
            {

                //layered safe protection
                if (context == null)
                    throw new ArgumentNullException(nameof(context), "HttpContext is null");

                if (string.IsNullOrWhiteSpace(context.Request?.Path))
                    throw new InvalidOperationException("Invalid request path");

                if (!context.Request.Scheme.Equals(_safeScheme, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Scheme '{context.Request.Scheme}' is not allowed");

                if (!context.Request.Host.Host.Equals(_safeHost, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Host '{context.Request.Host}' is not allowed");
                
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Request was cancelled.");
                    cancellationToken.ThrowIfCancellationRequested(); 
                }


                // Execute the pipeline and handle the request.
                 await _store.Value.Invoke(context);


            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Argument null while handling context."); ;
                throw; // Re-throws the caught exception to the caller (ProcessRequestAsync)
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during request pipeline execution.");
                throw; // Re-throws the caught exception to the caller (ProcessRequestAsync)
            }
        };



        public DefaultHttpContext BuildHttpContext(IFeatureCollection features)
        {

            var context =(DefaultHttpContext) _contextFactory.Value.Create(features);
            
            return context;
        }

        private bool IsNeedDisposal(HttpContext context)
        {
           if (Store.Get<bool>("__isblazor")) {
                if(context!=null && context.Request.Path.Equals("/")) {
                    return false;
                }
           }

           return context != null;
        }

        public void Dispose(DefaultHttpContext context)
        {
            var allowDisposal = IsNeedDisposal(context);
            if (allowDisposal)
            {
               _contextFactory?.Value.Dispose(context);
               context.Uninitialize();
               GC.SuppressFinalize(context);

            }
        }
    }

}
