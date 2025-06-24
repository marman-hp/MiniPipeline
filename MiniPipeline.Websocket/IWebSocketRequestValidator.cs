namespace MiniPipeline.WebSocket
{
    public interface IWebSocketRequestValidator
    {
        bool IsWebSocketUpgradeRequest(SocketRequest request, out string secWebSocketKey);
    }




}
