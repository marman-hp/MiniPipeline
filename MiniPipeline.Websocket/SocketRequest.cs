using Microsoft.AspNetCore.Http;

namespace MiniPipeline.WebSocket
{
    public class SocketRequest
    {
        public string Method { get; set; } = "";
        public string Path { get; set; } = "";
        public string Protocol { get; set; } = "";
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
    }

}
