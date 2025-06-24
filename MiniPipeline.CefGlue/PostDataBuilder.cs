using System.Diagnostics;
using Xilium.CefGlue;

namespace MiniPipeline.CefGlue
{

    public class DynamicByteBuffer
    {
        private byte[] _buffer;
        private int _count;

        public DynamicByteBuffer(int initialCapacity = 256)
        {
            _buffer = new byte[initialCapacity];
            _count = 0;
        }

        public int Length => _count;

        public byte[] ToArray()
        {
            var result = new byte[_count];
            Buffer.BlockCopy(_buffer, 0, result, 0, _count);
            return result;
        }

        public void Add(byte[] data, int offset, int count)
        {
            EnsureCapacity(_count + count);
            Buffer.BlockCopy(data, offset, _buffer, _count, count);
            _count += count;
        }

        private void EnsureCapacity(int size)
        {
            if (size > _buffer.Length)
            {
                int newSize = _buffer.Length * 2;
                while (newSize < size)
                    newSize *= 2;

                var newBuffer = new byte[newSize];
                Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _count);
                _buffer = newBuffer;
            }
        }
    }

    public static class PostDataBuilder
    {
        public static async Task<byte[]> GetBytesAsync(CefPostData postData, CancellationToken token)
        {
            if (postData == null)
                return Array.Empty<byte>();

            var buffer = new DynamicByteBuffer();
            var postDataElements = postData.GetElements();
            foreach (var element in postDataElements)
            {
                switch (element.ElementType)
                {
                    case CefPostDataElementType.Bytes:
                        var bytes = element.GetBytes();
                        buffer.Add(bytes, 0, bytes.Length);
                        break;

                    case CefPostDataElementType.File:
                        var path = element.GetFile();
                        if (File.Exists(path))
                        {
                            using var fs = File.OpenRead(path);
                            var tempBuffer = new byte[8192];
                            int read;
                            try
                            {
                                while ((read = await fs.ReadAsync(tempBuffer, 0, tempBuffer.Length, token)) > 0)
                                {
                                    buffer.Add(tempBuffer, 0, read);
                                };
                            }
                            catch (Exception)
                            {
                               Debug.WriteLine("transmit file was cancelled. CEF may have aborted the request.");
                               postData.RemoveAll();
                               postData.Dispose();
                               return Array.Empty<byte>();
                            }
                        }
                        break;
                }
            }


            // Cleanup resources
            postData.RemoveAll();
            postData.Dispose();

            return buffer.ToArray();
        }
        public static async Task<Stream> GetStreamAsync(CefPostData postData)
        {
            if (postData == null)
                return Stream.Null;

            var outputStream = new MemoryStream();

            var postDataElements = postData.GetElements();
            foreach (var element in postDataElements)
            {
                switch (element.ElementType)
                {
                    case CefPostDataElementType.Bytes:
                        var bytes = element.GetBytes();
                        await outputStream.WriteAsync(bytes, 0, bytes.Length);
                        break;

                    case CefPostDataElementType.File:
                        var path = element.GetFile();

                        if (File.Exists(path))
                        {
                            using var fs = File.OpenRead(path);
                            await fs.CopyToAsync(outputStream);
                        }
                        break;
                }
            }

            outputStream.Seek(0, SeekOrigin.Begin);

            // Cleanup resources
            postData.RemoveAll();
            postData.Dispose();

            return outputStream;
        }
    }
}
