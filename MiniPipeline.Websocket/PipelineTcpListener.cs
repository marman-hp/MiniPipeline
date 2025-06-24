using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace MiniPipeline.WebSocket
{
    public class PipelineTcpListener : ITcpListener
    {
        private readonly TcpListener _listener;
        public PipelineTcpListener(IOptions<PipelineSocketOptions> sslOptions)
        {
            _listener = new TcpListener(sslOptions.Value.IPAddress, sslOptions.Value.Port);
        }
        public void Start() => _listener.Start();
        public async Task<ITcpClient> AcceptTcpClientAsync(CancellationToken token)
        {
            var client = await _listener.AcceptTcpClientAsync(token);
            return new PipelineTcpClient(client);
        }
        public void Dispose() => _listener.Stop();
    }




}
