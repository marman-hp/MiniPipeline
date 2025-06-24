using System.Net.Sockets;

namespace MiniPipeline.WebSocket
{
    public class PipelineTcpClient : ITcpClient
    {
        private readonly TcpClient _client;
        public PipelineTcpClient(TcpClient client) => _client = client;
        public Stream GetStream() => _client.GetStream();
        public void Close() => _client.Close();
        public void Dispose() => _client.Dispose();
    }




}
