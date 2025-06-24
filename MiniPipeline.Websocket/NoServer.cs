using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace MiniPipeline.WebSocket
{

    public class NoServer : IServer
    {
        public IFeatureCollection Features =>  new FeatureCollection();

        public void Dispose()
        {
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken) where TContext : notnull
        => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
    }
}
