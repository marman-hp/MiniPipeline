namespace MiniPipeline.WebSocket
{
    public interface ITcpClient : IDisposable
    {
        Stream GetStream();
        void Close();
    }




}
