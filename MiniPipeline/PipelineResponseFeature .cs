using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.ObjectPool;
using System.IO.Pipelines;

namespace MiniPipeline.Core
{
 


    public class PipelineResponseFeature : IHttpResponseFeature, IHttpResponseBodyFeature, IDisposable
    {
        private readonly HeaderDictionary _headers = new HeaderDictionary();
        private readonly Action<Exception>? _abort;

        private Func<Task> _responseStartingAsync = () => Task.CompletedTask;
        private Func<Task> _responseCompletedAsync = () => Task.CompletedTask;

        private int _statusCode;
        private string? _reasonPhrase;
        private PipeWriter? _pipeWriter;
        private bool _completed;
        private bool _started;
        private bool _disposed;
        ObjectPool<MemoryStream>? _streamPool;
        public PipelineResponseFeature(ObjectPool<MemoryStream>? streamPool , Action<Exception>? abort = null)
        {
            _streamPool = streamPool;
            _abort = abort;
            Headers = _headers;
            if(_streamPool == null )
              Body = new MemoryStream();  //this default
            else
               Body = _streamPool!.Get(); //user have option to use pooled memory

            StatusCode = 200;  // Default HTTP status code
        }

        public int StatusCode
        {
            get => _statusCode;
            set
            {
                if (_started)
                {
                    throw new InvalidOperationException("The status code cannot be set, the response has already started.");
                }

                if (value < 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The status code cannot be set to a value less than 100");
                }

                _statusCode = value;
            }
        }

        public string? ReasonPhrase
        {
            get => _reasonPhrase;
            set
            {
                if (_started)
                {
                    throw new InvalidOperationException("The reason phrase cannot be set, the response has already started.");
                }

                _reasonPhrase = value;
            }
        }

        public IHeaderDictionary Headers { get; set; }

        public Stream Body { get; set; } = default!;

        public Stream Stream => Body;

        public PipeWriter Writer
        {
            get
            {
                if (_pipeWriter == null)
                {
                    _pipeWriter = PipeWriter.Create(Stream, new StreamPipeWriterOptions(leaveOpen: true));

                    // If response is already completed, mark pipeWriter as completed
                    if (_completed)
                    {
                        _pipeWriter.Complete();
                    }
                }

                return _pipeWriter;
            }
        }

        public bool HasStarted => _started;

        // Refactored: Chain the callbacks more simply
        public void OnStarting(Func<object, Task> callback, object state)
        {
            if (_started)
            {
                throw new InvalidOperationException("Cannot register callback after response has started.");
            }

            var prior = _responseStartingAsync;
            _responseStartingAsync = async () =>
            {
                await callback(state);
                await prior();
            };
        }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
            var prior = _responseCompletedAsync;

            _responseCompletedAsync = async () =>
            {
                try
                {
                    await callback(state);
                }
                finally
                {
                    await prior();
                }
            };
        }

        // Ensure headers are sent before starting response
        public async Task FireOnSendingHeadersAsync()
        {
            if (!_started)
            {
                await _responseStartingAsync();
                _started = true;
            }
        }

        // Called when response is completed
        public Task FireOnResponseCompletedAsync() => _responseCompletedAsync();

        // Start sending headers..
        public async Task StartAsync(CancellationToken token = default)
        {
            try
            {
                if (!_started)
                {
                    await _responseStartingAsync();
                    _started = true;
                    await Stream.FlushAsync(token);
                }
            }
            catch (Exception ex)
            {
                _abort?.Invoke(ex);
                throw;
            }
        }

        public void DisableBuffering()
        {
            Body = new NoBufferStream(Body);
        }

        // Send a file asynchronously
        public async Task SendFileAsync(string path, long offset, long? count, CancellationToken token)
        {
            if (!_started)
            {
                await StartAsync(token);
            }
            await SendFileFallback.SendFileAsync(Stream, path, offset, count, token);
        }

        // Complete the response and close stream
        public async Task CompleteAsync()
        {
            if (_disposed)
            {
                return;
            }
            if (_completed)
            {
                return;
            }

            if (!_started)
            {
                await StartAsync();
            }
            _completed = true;
         
            try
            {
                if (_pipeWriter != null)
                {
                    await _pipeWriter.CompleteAsync();
                }
            }
            finally
            {
                await FireOnResponseCompletedAsync();
            }

        }
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _pipeWriter?.Complete(); // non-async fallback

                if (_streamPool!=null)
                {
                  Body.SetLength(0); 
                  Body.Position = 0;
                  _streamPool.Return(Body as MemoryStream); // return and reuse pool
                }
                else
                {
                  Body?.Dispose();    
                  Body = Stream.Null; 
                }
            }
            catch
            {
                // swallow dispose exceptions (optional)
            }

        }
    }


    sealed class NoBufferStream : Stream
    {
        private readonly Stream _baseStream;

        public NoBufferStream(Stream baseStream)
        {
            _baseStream = baseStream;
        }

        public override void Flush() => _baseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

        public override void SetLength(long length) => _baseStream.SetLength(length);

        public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => _baseStream.CanWrite;

        public override long Length => _baseStream.Length;

        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }
    }

}
