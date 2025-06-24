using System.Collections.Specialized;

namespace MiniPipeline.Core
{
    public class PipelineRequest : IPipelineRequest
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public NameValueCollection Headers { get; set; }
        public byte[] Payloads { get; set;   }

        public void Dispose()
        {
            if (Payloads != null && Payloads.Length > 0)
            {
                Array.Clear(Payloads, 0, Payloads.Length);
                Payloads = null;
            }
        }
    }
}
