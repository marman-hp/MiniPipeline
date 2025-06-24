namespace MiniPipeline.WebSocket
{
    public interface IClientHandler
    {
        Task HandleClientAsync(ITcpClient client, CancellationToken cancellationToken);
    }




}
