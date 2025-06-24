using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;

namespace MiniPipeline.Core
{


    public abstract class PipelineProcessorBase : IPipelineProcessor, IDisposable
    {
        protected HttpContext? Context { get; private set; }
        protected CancellationTokenSource? ContextCts { get; private set; }
        protected readonly IHttpContextPipeline _pipeline;

        private readonly IEnumerable<IPipelineAddon> _addons;
        
        protected PipelineProcessorBase(IHttpContextPipeline pipeline,IEnumerable<IPipelineAddon> addons)
        {
            _addons = addons;
            _pipeline = pipeline;
            ContextCts = new();
        }
        public CancellationTokenSource CancellationTokenSource => ContextCts!;

        public virtual async Task<IPipelineResponse> OnProcessRequest(IFeatureCollection features, CancellationToken cefToken)
        {
            var response = new PipelineResponse();
            CancellationTokenSource? linkedTokenSource = null;

            try
            {
                Context = _pipeline.BuildHttpContext(features);
                foreach (var addon in _addons.OrderBy(x => x.Order))
                     await addon.OnBeforeExecute(Context);

                linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ContextCts!.Token);
                
                 await _pipeline.InvokeAsync(Context!, linkedTokenSource.Token);
                 await Context!.Response.CompleteAsync();


                linkedTokenSource?.Dispose();

                if (Context?.Response.Body == null || cefToken.IsCancellationRequested)
                {
                    return SetCanceled(response);
                }


                response.Stream = await CloneStreamAsync(Context.Response.Body, cefToken);

                if (response.Stream == null)
                {
                    return SetCanceled(response);
                }

                response.Headers = Context.Response.Headers.Clone();
                response.ContentType = Context.Response.ContentType;
                response.StatusCode = Context.Response.StatusCode;
                response.RequestPath = Context.Request.Path;

                foreach (var addon in _addons.OrderBy(x => x.Order)) {
                     if(cefToken.IsCancellationRequested)
                        break;
                     await addon.OnAfterExecute(response,cefToken);
                }


            }
            catch (Exception ex)
            {
                return SetCanceled(response);
            }

            return response;
        }




        protected async Task<Stream> CloneStreamAsync(Stream source, CancellationToken token)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var cloned = new MemoryStream();

            if (source.CanSeek)
                source.Position = 0;

            try
            {
                await source.CopyToAsync(cloned, token);
            }
            catch (Exception)
            {
                Debug.WriteLine("transmit stream was cancelled when Page executing. CEF may have aborted the request.");
                return null;
            }

            cloned.Position = 0;
            return cloned;
        }


    

        private IPipelineResponse SetCanceled(PipelineResponse response)
        {
            response.SetNull();
            return response;
        }





        public virtual void Cancel()
        {
            ContextCts?.Cancel();
        }

        public virtual void Dispose()
        {
            ContextCts?.Cancel();
            ContextCts?.Dispose();
            ContextCts = null;
            _pipeline?.Dispose(Context as DefaultHttpContext);
        }

    }
}
