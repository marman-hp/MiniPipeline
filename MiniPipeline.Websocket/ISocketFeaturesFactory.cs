using Microsoft.AspNetCore.Http.Features;

namespace MiniPipeline.WebSocket
{
    public interface ISocketFeaturesFactory
    {
         FeatureCollection CreateFeatures( SocketRequest socketRequest,Stream clientStream = null);
    }
}