using Microsoft.AspNetCore.Http.Features;
using System.Collections.Specialized;

namespace MiniPipeline.Core
{
    public interface IPipelineRequestBuilder
    {
         HttpRequestFeature CreateRequest(string url, NameValueCollection headers, string method, Stream body);

    }
}