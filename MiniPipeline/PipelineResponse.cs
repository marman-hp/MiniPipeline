using Microsoft.AspNetCore.Http;

namespace MiniPipeline.Core
{
    public class PipelineResponse : IPipelineResponse
    {

        bool _cancel = false;
        public Stream? Stream { get; set; }
        public IHeaderDictionary? Headers { get; set; }
        public string? ContentType { get; set; }
        public int StatusCode { get; set; }

        public PathString RequestPath { get; set; }
        public bool Cancel => _cancel;

        public void SetNull()
        {
            _cancel = true;
            Stream = null;
            Headers = null;
            ContentType = null;
            RequestPath = null;
        }
    }
}
