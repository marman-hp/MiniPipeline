using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Net.WebSockets;

namespace MiniPipeline.Core
{
    public sealed class SupportNegotiation : IPipelineAddon
    {
        public int Order => 1001;
        public  Task OnBeforeExecute(HttpContext context)
        { 
            if (context.Request.Path.Value?.Contains("negotiate", StringComparison.OrdinalIgnoreCase) == true)
            {
                context.Features.Set<IHttpWebSocketFeature>(new SignatureWebSocketFeature(context));
            }

            return  Task.CompletedTask;
        }

        public Task OnAfterExecute(PipelineResponse rawResponse, CancellationToken cancellationToken)
            => Task.CompletedTask;

    }

    internal sealed class SignatureWebSocketFeature : IHttpWebSocketFeature
    {
        private readonly HttpContext _context;

        public SignatureWebSocketFeature(HttpContext context)
        {
            _context = context;
        }

        public bool IsWebSocketRequest => true;

        public Task<WebSocket> AcceptAsync(WebSocketAcceptContext acceptContext)
        {
            throw new NotImplementedException();
        }

    }
}
