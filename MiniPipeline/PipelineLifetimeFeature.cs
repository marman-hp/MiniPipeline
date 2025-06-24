using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;

namespace MiniPipeline.Core
{
    public sealed  class PipelineLifetimeFeature : IHttpRequestLifetimeFeature
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public PipelineLifetimeFeature(CancellationTokenSource cts)
        {
            _cancellationTokenSource = cts;
            RequestAborted = _cancellationTokenSource.Token;
            
        }
        public CancellationToken RequestAborted { get; set; }

        public void Abort()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                Debug.WriteLine("IHttpRequestLifetimeFeature --> " + RequestAborted.GetHashCode().ToString());
                _cancellationTokenSource.Cancel();
            }

        }
 
    }

}
