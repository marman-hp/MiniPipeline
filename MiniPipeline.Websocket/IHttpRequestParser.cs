namespace MiniPipeline.WebSocket
{
    public interface IHttpRequestParser
    {
        SocketRequest Parse(string rawRequest);
    }




}
