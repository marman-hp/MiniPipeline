using Microsoft.AspNetCore.Http.Features;
using System.Collections.Specialized;

namespace MiniPipeline.Core
{
    public interface IPipelineFeaturesFactory : IDisposable
    {
        FeatureCollection CreateFeatures( IPipelineRequest pipelineRequest,CancellationTokenSource _cancellationToken);
    }
}