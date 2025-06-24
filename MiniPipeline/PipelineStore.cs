using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MiniPipeline.Core
{

    internal sealed class PipelineStore : IStore
    {

        Dictionary<string, dynamic> Items = new Dictionary<string, dynamic>();

        public  void Build(IApplicationBuilder app)
        {
            if (!Get("initialized", false) )
            {
                //App = _app;
                Invoke = app.Build();
                Set("initialized",true);
              }
        }

     
        public  RequestDelegate? Invoke { get; private set; }
        public  void Set(string key, dynamic value)
        {
            Items[key] = value;
        }

        public  T Get<T>(string key, T defaultValue = default)
        {
            if (Items.TryGetValue(key, out var value))
            {
                try
                {
                    if (value is T typedValue)
                        return typedValue;

                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    // Fallback to default on conversion error
                }
            }

            return defaultValue;
        }

    }

}
