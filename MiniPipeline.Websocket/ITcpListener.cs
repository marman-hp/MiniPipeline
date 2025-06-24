namespace MiniPipeline.WebSocket
{
    public interface ITcpListener : IDisposable
    {
        void Start();
        Task<ITcpClient> AcceptTcpClientAsync(CancellationToken token);
    }




}
