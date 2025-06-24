using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Security;
using System.Reflection;

namespace MiniPipeline.WebSocket
{
    public class DefaultSocketFeaturesFactory : ISocketFeaturesFactory
    {
        public FeatureCollection CreateFeatures(SocketRequest socketRequest, Stream clientStream = null)
        {
            FeatureCollection features = new FeatureCollection();


            var requestFeatures = new HttpRequestFeature()
            {
                Headers = socketRequest.Headers,
                Method = socketRequest.Method,
                Path = socketRequest.Path,
                Protocol = socketRequest.Protocol,
                Scheme = clientStream is SslStream ? "https" : "http"
            };

            features.Set<IHttpRequestFeature>(requestFeatures);

            var protectedStream = new WriteOnlyStream(clientStream);
            features.Set<IHttpResponseFeature>(new HttpResponseFeature(){ Body = protectedStream } );
            features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(protectedStream));

 
            return features;
        }


    }


    sealed class WriteOnlyStream : Stream
    {
        private readonly Stream _inner;

        public WriteOnlyStream(Stream innerStream)
        {
            _inner = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        }

        public override bool CanRead => false;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;

        public override bool CanTimeout => _inner.CanTimeout;

        public override long Length => _inner.Length;


        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override int WriteTimeout
        {
            get => _inner.WriteTimeout;
            set => _inner.WriteTimeout = value;
        }

        public override void Flush() => _inner.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) =>
            _inner.FlushAsync(cancellationToken);

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException("This stream is write-only.");

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
          => throw new NotSupportedException();

        public override ValueTask<int> ReadAsync(Memory<byte> memory, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _inner.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
        {
            return _inner.WriteAsync(source, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            _inner.WriteByte(value);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return _inner.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
            => _inner.EndWrite(asyncResult);

        public override void Close()
        {
            _inner.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }
        }
    }
}
