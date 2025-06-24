using Microsoft.Extensions.ObjectPool;

namespace MiniPipeline.Core
{
    //Only for testing purpose
    public static class SharedPools
    {
        public static readonly ObjectPool<MemoryStream> DefaultStreamPool = new DefaultObjectPoolProvider().Create(new MemoryStreamPooledObjectPolicy());
    }
}
