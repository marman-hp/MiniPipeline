using Xilium.CefGlue;
using System.Collections.Specialized;

namespace MiniPipeline.CefGlue
{
    public class CefRequestWrapper : ICefRequest
    {
        private readonly CefRequest _request;
        public CefRequestWrapper(CefRequest request) => _request = request;

        public string Method => _request.Method;
        public string Url => _request.Url;
        public NameValueCollection GetHeaderMap() => _request.GetHeaderMap();

        public string GetHeaderByName(string name) => _request.GetHeaderByName(name);
        

        public CefPostData PostData => _request.PostData;
    }
}
