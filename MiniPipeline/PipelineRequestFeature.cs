using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using System.Collections.Specialized;

namespace MiniPipeline.Core
{
    public class PipelineRequestFeature : IHttpRequestFeature, IDisposable
    {
        private bool _disposed = false;
        IPipelineRequest _pipelineRequest;
        public PipelineRequestFeature(IPipelineRequest pipelineRequest)
        {
            _pipelineRequest = pipelineRequest;
            var uri = new Uri(pipelineRequest.Url);


            Headers = new HeaderDictionary();
            Body = Stream.Null;
            Protocol = string.Empty;
            Scheme = uri.Scheme;
            Method = pipelineRequest.Method;
            PathBase = string.Empty;
            Path = uri.AbsolutePath;
            QueryString = uri.Query;
            RawTarget = string.Empty;

            if ((Method == "POST" || Method == "PUT" || Method == "PATCH") && _pipelineRequest.Payloads != Array.Empty<byte>())
            {
                Body = new ReadOnlyMemoryStream(_pipelineRequest.Payloads);
                Headers.TryAdd("Content-Length", new StringValues(Body.Length.ToString()));
            }
            else
            {
                Headers.TryAdd("Content-Length", "0");
            }

            var host = uri.Host + (uri.Port > 0 ? $":{uri.Port}" : "");
            Headers.TryAdd("Host", new StringValues(host));

            foreach (var key in pipelineRequest.Headers.AllKeys)
            {
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(pipelineRequest.Headers[key]))
                {
                    Headers.TryAdd(key, new StringValues(pipelineRequest.Headers[key]));
                }
            }

            Headers.TryAdd("Access-Control-Allow-Origin", new StringValues("*"));

        }
        public string Protocol { get; set; }
        public string Scheme { get; set; }
        public string Method { get; set; }
        public string PathBase { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string RawTarget { get; set; }
        public IHeaderDictionary Headers { get; set; }
        public Stream Body { get; set; }



        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (Body != Stream.Null)
            {
                Body.Dispose();
            }

            _pipelineRequest.Dispose();
            _pipelineRequest = null;
            GC.SuppressFinalize(this); 
        }
    }
}
