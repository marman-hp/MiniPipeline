namespace MiniPipeline.WebSocket
{
    public class WebSocketRequestValidator : IWebSocketRequestValidator
    {
        public bool IsWebSocketUpgradeRequest(SocketRequest request, out string secWebSocketKey)
        {
            secWebSocketKey = null;

            if (request.Headers.TryGetValue("Upgrade", out var upgradeValue) &&
                upgradeValue.ToString().Equals("websocket", StringComparison.OrdinalIgnoreCase) &&
                request.Headers.TryGetValue("Connection", out var connectionValue) &&
                connectionValue.ToString().Contains("Upgrade", StringComparison.OrdinalIgnoreCase) &&
                request.Headers.TryGetValue("Sec-WebSocket-Key", out var key))
            {
                secWebSocketKey = key.ToString().Trim();
                return true;
            }

            return false;
        }
    }




}
