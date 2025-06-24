using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MiniPipeline.Core
{
    public interface IStore {
        void Build(IApplicationBuilder app);
        RequestDelegate Invoke { get;}
        void Set(string key, dynamic value);
        T Get<T>(string key, T  defaultvalue = default);
    }

}
