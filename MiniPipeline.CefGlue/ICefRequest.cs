using Xilium.CefGlue;
using System.Collections.Specialized;

namespace MiniPipeline.CefGlue
{
    public interface ICefRequest
    {
        string Method { get; }
        string Url { get; }
        NameValueCollection GetHeaderMap();
        string GetHeaderByName(string name);
        CefPostData PostData { get; }
    }
}
