using System.Collections.Specialized;

namespace MiniPipeline.Core
{
    public interface IPipelineRequest : IDisposable
    {
        string Url { get; set;}
        string Method { get; set;}
        NameValueCollection Headers { get; set;}
        byte[] Payloads { get; set;}
    }
}
