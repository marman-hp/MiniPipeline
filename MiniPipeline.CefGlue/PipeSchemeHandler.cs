using System.Diagnostics;
using System.Collections.Specialized;
using Microsoft.Extensions.DependencyInjection;
using Xilium.CefGlue;
using MiniPipeline.Core;

namespace MiniPipeline.CefGlue
{
    public class PipeSchemeHandler : CefResourceHandler
    {
        private long _responseStreamReadPosition = -1L;


        private IPipelineResponse? pipelineResponse;
        private readonly IPipelineProcessor _processor;
        private readonly TaskCompletionSource<bool> _cancelDone = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly IServiceScope scope;
        private readonly IPipelineFeaturesFactory featuresFactory;
        private readonly CancellationTokenSource _ceftokenSource;
        public PipeSchemeHandler(IServiceProvider provider)
        {
            scope = provider.CreateScope();
            _ceftokenSource = new CancellationTokenSource();
            _processor = scope.ServiceProvider.GetRequiredService<IPipelineProcessor>();
            featuresFactory = scope.ServiceProvider.GetRequiredService<IPipelineFeaturesFactory>();
            pipelineResponse = new PipelineResponse();
        }




        internal async Task ProcessRequestAsync(ICefRequest request, ICefCallback callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            var features = featuresFactory.CreateFeatures(new PipelineRequest()
            {
                Url = request.Url,
                Headers = request.GetHeaderMap() ?? new NameValueCollection(),
                Method = request.Method,
                Payloads = request.Method == "POST" ||
                           request.Method == "PUT" ||
                           request.Method == "PATCH"
                ? await PostDataBuilder.GetBytesAsync(request.PostData, _ceftokenSource.Token)
                : Array.Empty<byte>()
            }, _processor.CancellationTokenSource);

            pipelineResponse = await _processor.OnProcessRequest(features, _ceftokenSource.Token);

            if (!pipelineResponse.Cancel)
                callback.Continue();

            callback.Dispose();




        }


        protected override bool Open(CefRequest request, out bool handleRequest, CefCallback callback)
        {

            var task = Task.Run(async () =>
            {
                await ProcessRequestAsync(new CefRequestWrapper(request), new CefCallbackWrapper(callback));
                await _cancelDone.Task; //wait signal cancel from CEF
            }
             );


            task.ContinueWith(t =>
            {
                pipelineResponse?.SetNull();
                scope.Dispose();
                _processor.Cancel();
            }, TaskScheduler.Default); // safe to dispose now

            handleRequest = false;

            return true;
        }

        protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
        {
            if (pipelineResponse.Stream == null)
            {
                responseLength = 0;
                redirectUrl = "";
                response.Status = 500;
                response.StatusText = "Internal Server Error";
                response.MimeType = "text/plain";
                return;
            }

            //bool hasCookies = false;
            foreach (var header in pipelineResponse.Headers)
            {
                if (header.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
                {
                    //hasCookies = true;
                    foreach (var cookieValue in header.Value)
                        response.SetHeaderByName("Set-Cookie", cookieValue, false); // false = allow multiple cookies
                }
                else
                    response.SetHeaderByName(header.Key, header.Value, true);
            }



            if (pipelineResponse.ContentType == "text/html; charset=utf-8")
                response.SetHeaderByName("Content-Type", "text/html; charset=utf-8", true);


            responseLength = -1L;
            redirectUrl = "";

            response.Status = pipelineResponse.StatusCode;
            response.StatusText = Helper.GetStatus(pipelineResponse.StatusCode);

            // Set mime type based on response content type
            response.MimeType = !string.IsNullOrEmpty(pipelineResponse.ContentType)
                ? pipelineResponse.ContentType.Contains("; charset=utf-8")
                    ? pipelineResponse.ContentType.Split(";")[0]
                    : pipelineResponse.ContentType
                : "text/plain";
            if (pipelineResponse.Stream != null && pipelineResponse.Stream.CanSeek)
                responseLength = pipelineResponse.Stream.Length;


        }



        protected override bool Read(Stream response, int bytesToRead, out int bytesRead, CefResourceReadCallback callback)
        {

            callback?.Dispose();

            InitializeStreamPositionIfNeeded();

            if (pipelineResponse.Stream == null)
            {
                bytesRead = -2;
                return false;
            }

            byte[] buffer = new byte[bytesToRead];
            lock (pipelineResponse.Stream)
            {
                if (pipelineResponse.Stream.Position != _responseStreamReadPosition)
                {
                    if (!pipelineResponse.Stream.CanSeek)
                    {
                        bytesRead = -2;
                        return false;
                    }

                    pipelineResponse.Stream.Position = _responseStreamReadPosition;
                }


                bytesRead = pipelineResponse.Stream.Read(buffer, 0, buffer.Length);
                _responseStreamReadPosition = pipelineResponse.Stream.Position;
            }



            if (bytesRead == 0)
            {
                return false;
            }

            // remove uncomment for download simulation ONLY to see the progress 
            // without this, the download progress is too fast
            //if (responseHeader.TryGetValue("Content-Disposition", out var cd) == true &&
            //    cd.ToString().Contains("attachment", StringComparison.OrdinalIgnoreCase))
            //{
            //    // Simulate slow transfer (e.g., 2ms per KB)
            //    int delay = Math.Max(1, bytesToRead / 512);
            //    Thread.Sleep(delay);
            //}

            response.Write(buffer, 0, bytesRead);
            return bytesRead > 0;
        }

        protected override bool Skip(long bytesToSkip, out long bytesSkipped, CefResourceSkipCallback callback)
        {


            InitializeStreamPositionIfNeeded();

            if (pipelineResponse.Stream == null || !pipelineResponse.Stream.CanSeek)
            {
                bytesSkipped = -2L;
                return false;
            }

            bytesSkipped = bytesToSkip;
            lock (pipelineResponse.Stream)
            {
                _responseStreamReadPosition += bytesToSkip;
            }

            return true;
        }


        protected override void Cancel()
        {
            _ceftokenSource.Cancel();

            //_processor.Cancel();           //enable this the .net will close entire process before finishing
            _cancelDone.TrySetResult(true);
        }

        private void InitializeStreamPositionIfNeeded()
        {
            Interlocked.CompareExchange(ref _responseStreamReadPosition, 0L, -1L);
        }



    }
}
