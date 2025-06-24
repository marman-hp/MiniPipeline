using Microsoft.Extensions.ObjectPool;

namespace MiniPipeline.Core
{
    public class MemoryStreamPooledObjectPolicy: PooledObjectPolicy<MemoryStream>
    {
        public override MemoryStream Create() => new MemoryStream();

        public override bool Return(MemoryStream stream)
        {
            if(stream != null) {
            stream.SetLength(0);
            stream.Position = 0;
            }
            return stream != null;

        }

    }
}
