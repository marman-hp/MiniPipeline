using Microsoft.AspNetCore.Http;

namespace MiniPipeline.Core
{
    public interface IPipelineResponse
    {
        Stream? Stream { get; set; }
        IHeaderDictionary? Headers { get; set;}
        string? ContentType { get; set;}
        int StatusCode { get; set;}
         PathString RequestPath { get; set; }
        bool Cancel { get;}
        void SetNull();
    }
}
