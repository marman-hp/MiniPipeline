using Microsoft.AspNetCore.Http;

namespace MiniPipeline.Core
{
    public interface IPipelineAddon
    {
      int Order { get; }
      Task OnBeforeExecute(HttpContext context);
      Task OnAfterExecute(PipelineResponse rawResponse, CancellationToken cancellationToken);
    }
}
