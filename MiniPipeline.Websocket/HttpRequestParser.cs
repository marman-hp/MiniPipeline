namespace MiniPipeline.WebSocket
{
    public class HttpRequestParser : IHttpRequestParser
    {
        public SocketRequest Parse(string httpRequestText)
        {
            var request = new SocketRequest();

            using (var reader = new StringReader(httpRequestText))
            {
                // Parse request line: e.g. "GET /ws HTTP/1.1"
                var requestLine = reader.ReadLine();
                if (requestLine == null)
                    throw new InvalidOperationException("Invalid HTTP request: empty");

                var parts = requestLine.Split(' ');
                if (parts.Length != 3)
                    throw new InvalidOperationException("Invalid HTTP request line: " + requestLine);

                request.Method = parts[0];
                request.Path = parts[1];
                request.Protocol = parts[2];

                // Parse headers
                string? line;
                while ((line = reader.ReadLine()) != null && line != string.Empty)
                {
                    var colonIndex = line.IndexOf(':');
                    if (colonIndex == -1) continue;

                    var name = line.Substring(0, colonIndex).Trim();
                    var value = line.Substring(colonIndex + 1).Trim();

                    request.Headers[name] = value;
                }
            }

            return request;

        }
    }




}
