using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.ObjectPool;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;

namespace MiniPipeline.Core
{


    public class DefaultPipelineFeaturesFactory : IPipelineFeaturesFactory
    {
        ObjectPool<MemoryStream>? _defaultStreamPool;
        PipelineRequestFeature? requestFeature;
        PipelineResponseFeature? responseFeature;
        public DefaultPipelineFeaturesFactory(ObjectPool<MemoryStream>? defaultStreamPool=null) { 
            _defaultStreamPool = defaultStreamPool ?? SharedPools.DefaultStreamPool; 
        }

        public FeatureCollection CreateFeatures( IPipelineRequest pipelineRequest,CancellationTokenSource _cancellationTokenSource = default)
        {

            FeatureCollection features = new FeatureCollection();



            var cookiesFeature = new ResponseCookiesFeature(features);
            features.Set<IResponseCookiesFeature>(cookiesFeature);


         
            requestFeature = new PipelineRequestFeature(pipelineRequest);

            features.Set<IHttpRequestFeature>(requestFeature);
            features.Set<IHttpRequestLifetimeFeature>(new PipelineLifetimeFeature(_cancellationTokenSource));


            responseFeature = new PipelineResponseFeature(_defaultStreamPool,ex =>
            {
                Debug.WriteLine("Abort response: " + ex.Message);
            });
            

            features.Set<IHttpResponseFeature>(responseFeature);
            features.Set<IHttpResponseBodyFeature>(responseFeature);

            var queryDict = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(requestFeature.QueryString);
            features.Set<IQueryFeature>(new QueryFeature(new QueryCollection(queryDict)));

            //var upgradeFeature = new SocketUpgradeFeature(stream, () => context,);

           // Uri uri = new Uri(pipelineRequest.Url);

            return features;
        }

        public void Dispose()
        {
            responseFeature?.Dispose();
            requestFeature?.Dispose();
        }
    }


}
