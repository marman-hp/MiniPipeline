using Microsoft.AspNetCore.Http.Features;

namespace MiniPipeline.Core
{
    public interface IPipelineProcessor : IDisposable
    {
       Task<IPipelineResponse> OnProcessRequest(IFeatureCollection features, CancellationToken cefToken);

       CancellationTokenSource CancellationTokenSource { get; }
       void Cancel(); // token cancellation

    }
}
