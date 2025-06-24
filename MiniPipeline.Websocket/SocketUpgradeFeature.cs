using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MiniPipeline.WebSocket
{
    internal sealed class SocketUpgradeFeature : IHttpUpgradeFeature
    {
        private readonly Stream _stream;
        private bool _accepted;
        private readonly HttpContext _context;
        private readonly CancellationToken _token;
        public SocketUpgradeFeature(Stream stream,HttpContext context,CancellationToken? token = default )
        {
            _stream = stream;
            _context = context;
            _token = token ?? default;
        }

        public bool IsUpgradableRequest => true;

        
        public async Task<Stream> UpgradeAsync()
        {

            if (_accepted)
               throw new Exception("The upgrade only called once."); 

            _accepted = true;

            
            var response = new StringBuilder();
            response.AppendLine("HTTP/1.1 101 Switching Protocols");

            foreach (var header in _context.Response.Headers)
            {
                foreach (var value in header.Value)
                {
                    response.AppendLine($"{header.Key}: {value}");
                }
            }

             var subProtocol = _context.Response.Headers["Sec-WebSocket-Protocol"].ToString();



            response.AppendLine();

            var bytes = Encoding.ASCII.GetBytes(response.ToString());
            await _stream.WriteAsync(bytes, 0, bytes.Length);
          



            return _stream;

        }

    }

}
