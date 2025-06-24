namespace MiniPipeline.Core
{
    public sealed class ReadOnlyMemoryStream : MemoryStream
    {
        public ReadOnlyMemoryStream(byte[] buffer)
            : base(buffer, 0, buffer.Length, writable: false, publiclyVisible: true)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("This stream is read-only.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("This stream is read-only.");
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("This stream is read-only.");
        }
    }
}
