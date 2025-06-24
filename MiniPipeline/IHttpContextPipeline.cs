using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Collections.Specialized;

namespace MiniPipeline.Core
{
    public interface IHttpContextPipeline
    {
        //IFeatureCollection BuildFeatures(IPipelineRequest pipelineRequest, CancellationTokenSource cts);
        DefaultHttpContext BuildHttpContext(IFeatureCollection features);

        Func<HttpContext, CancellationToken, Task> InvokeAsync { get; }
        Func<HttpContext,  Task> SocketInvokeAsync { get; }


        void Dispose(DefaultHttpContext context);
    }

}
